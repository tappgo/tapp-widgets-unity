using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using TappGo.Widgets.Internal;
using UnityEditor.Build;

namespace TappGo.Widgets.Editor
{
    /// <summary>
    /// The pure half of the Android build hook: what gets written into <c>unityLibrary</c>, given paths and
    /// values. No Unity API, so it is unit-tested against temporary folders.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Three jobs, three methods. <see cref="StageWidgets"/> writes one provider class and one manifest
    /// fragment per widget id plus the shared resources; <see cref="MergeManifest"/> puts those fragments
    /// into Unity's generated manifest as XML, replacing whatever an earlier build put there;
    /// <see cref="WriteGradleDependency"/> appends the Maven dependency to the module's <c>build.gradle</c>
    /// as one owned, anchored block.
    /// </para>
    /// <para>
    /// <b>Everything is idempotent by name.</b> Unity regenerates the Gradle project on every build, but a
    /// host's own <c>mainTemplate.gradle</c> or an incremental Unity 6 build can hand this hook a file it
    /// already edited, so every write first removes what this hook owns and then writes it again.
    /// </para>
    /// </remarks>
    internal static class AndroidWidgetStaging
    {
        /// <summary>Marks a placeholder, and makes an unsubstituted one greppable.</summary>
        private const string TokenPrefix = "__TAPP_";

        private const string AndroidNamespace = "http://schemas.android.com/apk/res/android";

        /// <summary>The resources every staged widget shares, relative to <c>src/main</c>.</summary>
        private static readonly string[] SharedResources =
        {
            "res/values/tapp_widget_strings.xml",
            "res/xml/tapp_widget_info.xml",
            "res/xml-v31/tapp_widget_info.xml",
        };
        internal const string ExactAlarmPermission = "android.permission.SCHEDULE_EXACT_ALARM";

        /// <summary>One widget to stage: the campaign id and the values every token takes for it.</summary>
        internal readonly struct Widget
        {
            internal string Id { get; }
            internal string ClassName { get; }

            internal Widget(string id)
            {
                Id = id;
                ClassName = AndroidWidgetNaming.ClassName(id);
            }
        }

        /// <summary>What a staging run wrote, relative to the module root.</summary>
        internal sealed class Staged
        {
            internal List<string> Files { get; } = new List<string>();
            internal List<string> ReceiverFragments { get; } = new List<string>();
        }

        /// <summary>
        /// Writes the provider classes, the shared resources, and returns the manifest fragments to merge.
        /// </summary>
        /// <param name="templateRoot">The package's <c>Runtime/AndroidWidgetTemplate~</c>.</param>
        /// <param name="moduleRoot">The generated <c>unityLibrary</c> folder.</param>
        /// <param name="widgets">One per configured widget id, in the settings' order.</param>
        /// <param name="shared">The values for the tokens every file shares: label, description, period.</param>
        /// <exception cref="BuildFailedException">If a <c>__TAPP_…__</c> placeholder survives.</exception>
        internal static Staged StageWidgets(
            string templateRoot,
            string moduleRoot,
            IReadOnlyList<Widget> widgets,
            IReadOnlyDictionary<string, string> shared)
        {
            var staged = new Staged();
            var javaFolder = Path.Combine(moduleRoot, "src", "main", "java",
                AndroidWidgetNaming.Package.Replace('.', Path.DirectorySeparatorChar));

            // Ours from a previous run, by name — never anything else in the host's java tree.
            if (Directory.Exists(javaFolder))
            {
                foreach (var stale in Directory.GetFiles(javaFolder, "TappWidgetProvider_*.java"))
                {
                    File.Delete(stale);
                }
            }

            if (widgets.Count == 0)
            {
                // The package is installed for Picture in Picture alone: no widget, so no provider, no widget
                // description and no label for a picker that will list nothing. What an earlier build with ids
                // left behind goes too — a description with no receiver naming it is only dead weight, but a
                // stale file is how a later reader concludes the build still has widgets.
                foreach (var resource in SharedResources)
                {
                    // Checked first: a fresh export has no res/xml folder at all, and on Windows File.Delete
                    // throws for a missing folder where it is silent for a missing file.
                    var relative = resource.Replace('/', Path.DirectorySeparatorChar);
                    var stale = Path.Combine(moduleRoot, "src", "main", relative);
                    if (File.Exists(stale))
                    {
                        File.Delete(stale);
                    }
                }

                return staged;
            }

            Directory.CreateDirectory(javaFolder);

            var javaTemplate = File.ReadAllText(Path.Combine(templateRoot, "java", "TappWidgetProvider.java"));
            var receiverTemplate = File.ReadAllText(Path.Combine(templateRoot, "manifest", "receiver.xml"));

            foreach (var widget in widgets)
            {
                var tokens = new Dictionary<string, string>(shared)
                {
                    ["__TAPP_PROVIDER_CLASS__"] = widget.ClassName,
                    ["__TAPP_WIDGET_ID__"] = widget.Id,
                };

                var javaPath = Path.Combine(javaFolder, widget.ClassName + ".java");
                File.WriteAllText(javaPath, Substitute(javaTemplate, tokens, "TappWidgetProvider.java"));
                staged.Files.Add(Relative(moduleRoot, javaPath));
                staged.ReceiverFragments.Add(Substitute(receiverTemplate, tokens, "receiver.xml"));
            }

            foreach (var resource in SharedResources)
            {
                var source = Path.Combine(templateRoot, resource.Replace('/', Path.DirectorySeparatorChar));
                var target = Path.Combine(moduleRoot, "src", "main", resource.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(target) ?? moduleRoot);
                File.WriteAllText(target, Substitute(File.ReadAllText(source), shared, resource));
                staged.Files.Add(Relative(moduleRoot, target));
            }

            return staged;
        }

        /// <summary>
        /// Puts the receivers into the module's manifest, replacing any this hook wrote before.
        /// </summary>
        /// <remarks>
        /// XML, never text: the manifest is Unity's, other hooks edit it too, and a string patch that matched
        /// the wrong element would corrupt a file the host never wrote. Only elements this hook owns — a
        /// receiver whose class is in <see cref="AndroidWidgetNaming.Package"/> — are ever removed. The
        /// exact-alarm permission is added with the first receiver and never taken away again: a host may hold
        /// it for reasons of its own, and nothing in the file says whose declaration it is.
        /// </remarks>
        internal static void MergeManifest(string manifestPath, IReadOnlyList<string> receiverFragments)
        {
            var document = new XmlDocument { PreserveWhitespace = true };
            document.Load(manifestPath);

            var manifest = document.DocumentElement;
            var application = manifest?.Name == "manifest"
                ? manifest.SelectSingleNode("application") as XmlElement
                : null;
            if (application == null)
            {
                if (receiverFragments.Count == 0)
                {
                    // Nothing to add, and nowhere an earlier run could have added to: not this hook's to judge.
                    return;
                }

                throw new BuildFailedException(manifest?.Name == "manifest"
                    ? $"[Tapp] {manifestPath} has no <application> element to add the widget to."
                    : $"[Tapp] {manifestPath} has no <manifest> root.");
            }

            var namespaces = new XmlNamespaceManager(document.NameTable);
            namespaces.AddNamespace("android", AndroidNamespace);

            var removed = 0;
            foreach (var owned in application.SelectNodes("receiver", namespaces).Cast<XmlElement>().ToList())
            {
                var name = owned.GetAttribute("name", AndroidNamespace);
                if (name.StartsWith(AndroidWidgetNaming.Package + ".", StringComparison.Ordinal))
                {
                    application.RemoveChild(owned);
                    removed++;
                }
            }

            if (receiverFragments.Count == 0)
            {
                // No widget this build. Receivers an earlier build declared are gone above; the file is written
                // only if that changed it, and the exact-alarm permission is not added for a widget that does
                // not exist. One an earlier build added stays: nothing marks it as ours rather than the host's.
                if (removed > 0)
                {
                    document.Save(manifestPath);
                }

                return;
            }

            foreach (var fragment in receiverFragments)
            {
                var piece = new XmlDocument();
                piece.LoadXml(fragment);
                var imported = document.ImportNode(piece.DocumentElement, true);
                application.AppendChild(document.CreateWhitespace("\n    "));
                application.AppendChild(imported);
            }

            application.AppendChild(document.CreateWhitespace("\n  "));

            // Recommended by the native SDK, not required: without it countdown-end alarms are inexact and a
            // widget can count past zero for minutes. Declared once; a host that already has it keeps its own.
            var declared = manifest.SelectNodes("uses-permission", namespaces).Cast<XmlElement>()
                .Any(p => p.GetAttribute("name", AndroidNamespace) == ExactAlarmPermission);
            if (!declared)
            {
                var permission = document.CreateElement("uses-permission");
                permission.SetAttribute("name", AndroidNamespace, ExactAlarmPermission);
                manifest.InsertBefore(permission, application);
                manifest.InsertBefore(document.CreateWhitespace("\n  "), application);
            }

            document.Save(manifestPath);
        }

        /// <summary>Replaces every token, then proves none is left.</summary>
        private static string Substitute(string content, IReadOnlyDictionary<string, string> tokens, string fileName)
        {
            foreach (var token in tokens)
            {
                content = content.Replace(token.Key, token.Value);
            }

            var leftover = content.IndexOf(TokenPrefix, StringComparison.Ordinal);
            if (leftover >= 0)
            {
                var end = content.IndexOf("__", leftover + TokenPrefix.Length, StringComparison.Ordinal);
                var name = end < 0 ? TokenPrefix : content.Substring(leftover, end + 2 - leftover);
                throw new BuildFailedException(
                    $"[Tapp] {name} was not substituted in {fileName}. The package's Android template and its build " +
                    "hook are out of step — reinstall the package rather than editing the staged copy.");
            }

            return content;
        }

        private static string Relative(string root, string path)
        {
            var trimmed = path.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar, '/');
            return trimmed.Replace(Path.DirectorySeparatorChar, '/');
        }

        /// <summary>Escapes a value for an Android string resource: <c>&amp;</c>, <c>&lt;</c>, quotes and apostrophes.</summary>
        internal static string ResourceString(string value)
        {
            return (value ?? "")
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "\\\"")
                .Replace("'", "\\'");
        }
    }
}
