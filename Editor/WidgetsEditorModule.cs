using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TappGo.Editor.Modules;
using TappGo.Modules;
using TappGo.Widgets.Internal;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace TappGo.Widgets.Editor
{
    /// <summary>
    /// What widgets need from the Editor: a section on the settings page, an iOS build that links the module
    /// and renders the widgets, and an Android build that creates one home-screen widget per campaign. With no
    /// widget id — the package installed for Picture in Picture alone — both builds link the module and stage
    /// nothing else.
    /// </summary>
    /// <remarks>
    /// The iOS half is data core's build hook applies. The Android half names its Maven artifact and hands
    /// core a step to run once the Gradle project exists — staging sources, resources and manifest receivers
    /// is this package's own, and <see cref="AndroidWidgetStaging"/> does it.
    /// </remarks>
    [InitializeOnLoad]
    internal sealed class WidgetsEditorModule : TappEditorModule
    {
        /// <summary>The Swift package product, and the module the generated bundle imports.</summary>
        internal const string Product = "TappWidgets";

        /// <summary>The Maven artifact under <c>com.tappgo</c>; it brings <c>tapp-core</c> with it.</summary>
        internal const string AndroidArtifact = "tapp-widgets";

        internal const string AppTemplatePath = "Runtime/iOSAppTemplate~";
        internal const string AndroidTemplatePath = "Runtime/AndroidWidgetTemplate~";

        static WidgetsEditorModule()
        {
            TappEditorModules.Register(new WidgetsEditorModule());

            // The runtime module adds itself when a player starts, which the Editor only does in Play mode.
            // The settings page and the build hook validate through the same registry, so without this they
            // ask no product anything: a project with no widget ids reads "Ready" and a batch build passes.
            TappModules.Register(new WidgetsModule());
        }

        public override string Id => WidgetsModule.PackageName;

        public override string DisplayName => "Widgets";

        public override ScriptableObject Settings(bool create)
        {
            return TappModuleSettings.LoadOrCreate<TappWidgetsSettings>(TappWidgetsSettings.ResourceName, create, Seed);
        }

        /// <summary>Carries over what a pre-split <c>TappGoSettings.asset</c> had set for this product.</summary>
        internal static void Seed(TappWidgetsSettings created)
        {
            Seed(created, TappModuleSettings.Legacy());
        }

        internal static void Seed(TappWidgetsSettings created, IReadOnlyDictionary<string, string> legacy)
        {
            if (legacy.TryGetValue("widgetIds", out var ids) && ids.Length > 0)
            {
                created.widgetIds = ids.Split('\n');
            }
            else
            {
                // A host starting out: the docs lead with one small widget, so a new asset offers that size
                // alone. Only a new asset — the field's default stays both sizes, so every asset made before the
                // field existed builds what it always built, and so does one carried over from a pre-split
                // project with widget ids, whose players may have placed either size.
                created.iosWidgetSizes = TappWidgetSizes.Small;
            }

            if (legacy.TryGetValue("pictureInPictureEnabled", out var pictureInPicture))
            {
                created.pictureInPictureEnabled = pictureInPicture == "1";
            }

            if (legacy.TryGetValue("androidWidgetLabel", out var label))
            {
                created.androidWidgetLabel = label;
            }

            if (legacy.TryGetValue("androidWidgetDescription", out var description))
            {
                created.androidWidgetDescription = description;
            }

            if (legacy.TryGetValue("androidWidgetUpdatePeriodMillis", out var period) &&
                int.TryParse(period, NumberStyles.Integer, CultureInfo.InvariantCulture, out var millis))
            {
                created.androidWidgetUpdatePeriodMillis = millis;
            }
        }

        public override TappIosContribution Ios(TappGoSettings settings)
        {
            var own = WidgetsModule.Own();
            var ids = own == null ? new string[0] : own.ConfiguredWidgetIds();
            var contribution = new TappIosContribution();

            // Always, widgets or not: linking the product into the app is what loads it there, and Picture in
            // Picture and the pending link are answered from the app. Unlinked, every call is surfaceNotLinked.
            contribution.SwiftProducts.Add(Product);

            if (own != null && own.pictureInPictureEnabled)
            {
                // What keeps a Picture in Picture session playing after the player leaves the app — and a mode
                // App Review asks about, so it is added for the hosts that asked and no one else.
                contribution.AppBackgroundModes.Add("audio");
            }

            if (ids.Length == 0)
            {
                // Installed for Picture in Picture alone. There is no widget to render, so nothing is asked of
                // the extension — not its bundle, not its frameworks, not its Keychain — and when no other
                // product renders either, core creates no extension target at all. Handing the SDK an empty
                // id list there is the one call it always refuses.
                return contribution;
            }

            // A widget authenticates and fetches on its own when WidgetKit renders it, and decrypts what it
            // cached with a key kept in the Keychain under the App Group. Without the access group every
            // widget renders its placeholder for ever — on a device only, since the Simulator enforces no
            // entitlement.
            contribution.ExtensionKeychainSharing = true;
            contribution.ExtensionFrameworks.Add("AppIntents.framework");

            // An interactive widget's tap that opens the app is performed by the app, and iOS only routes it
            // to an app that declares the SDK's intents package — omit it and a CLAIM button does nothing at
            // all, with no log. The extension declares the same package for the taps it performs itself.
            contribution.AppSourcesRoot = TappEditorModules.PackageFolder(typeof(WidgetsEditorModule).Assembly, AppTemplatePath);

            contribution.BundleImports.Add("AppIntents");
            contribution.BundleImports.Add(Product);
            contribution.BundleSetup.Add(ExtensionSetup(settings.appGroupIdentifier, own));
            contribution.BundleBody.AddRange(BundleBody(ids.Length, own.iosWidgetSizes));
            contribution.BundleDeclarations.Add(IntentsDeclaration);
            return contribution;
        }

        /// <summary>
        /// What the extension's <c>body</c> declares: both sizes for every id, or one size per id in id order.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Both sizes are <c>TappWidgets.all</c>, exactly as before the setting existed, so an existing project
        /// generates the same file. One size is one entry per id — slot 0 takes no argument — because a
        /// WidgetKit kind is derived from slot and size alike either way, and that is what keeps the widgets a
        /// player placed at the size that stays.
        /// </para>
        /// <para>
        /// No size is refused by <see cref="WidgetsModule.Validate"/> before a build gets here; should one
        /// arrive anyway it is read as both, today's output, rather than as a bundle that declares nothing.
        /// </para>
        /// </remarks>
        internal static IEnumerable<string> BundleBody(int widgets, TappWidgetSizes sizes)
        {
            sizes &= TappWidgetsSettings.AllIosWidgetSizes;
            if (sizes == TappWidgetSizes.Small || sizes == TappWidgetSizes.Medium)
            {
                var type = sizes == TappWidgetSizes.Small ? "TappSmallWidget" : "TappMediumWidget";
                return Enumerable.Range(0, widgets).Select(slot => slot == 0 ? $"{type}()" : $"{type}(slot: {slot})");
            }

            return new[] { "TappWidgets.all" };
        }

        /// <summary>
        /// The statement that hands the Widgets module its ids and the App Group, inside the extension.
        /// </summary>
        /// <remarks>
        /// The ids passed validation, so each is a UUID and none needs escaping. Their <b>order</b> is written
        /// exactly as the settings list it: WidgetKit knows a placed widget by its position in this list.
        /// </remarks>
        internal static string ExtensionSetup(string appGroup, TappWidgetsSettings own)
        {
            var ids = own == null ? new string[0] : own.ConfiguredWidgetIds();
            var literal = string.Join(", ", ids.Select(id => $"\"{id}\""));
            return "try? TappWidgets.configureExtension(\n" +
                   $"    TappWidgetConfiguration(widgetIDs: [{literal}],\n" +
                   $"                            appGroupIdentifier: \"{appGroup}\")\n" +
                   ")";
        }

        private const string IntentsDeclaration =
            "/// The taps a widget performs itself — a spin, a scratch — are App Intents this extension runs.\n" +
            "/// Declaring the SDK's package is the whole registration; there are no intent types of ours to add.\n" +
            "/// The app target declares the same package for the taps that open it (TappIntentsPackage.swift).\n" +
            "struct TappExtensionIntentsPackage: AppIntentsPackage {\n" +
            "    static var includedPackages: [any AppIntentsPackage.Type] { [TappWidgetIntentsPackage.self] }\n" +
            "}\n";

        public override TappAndroidContribution Android(TappGoSettings settings)
        {
            var contribution = new TappAndroidContribution { PostGenerate = StageAndroidWidgets };
            contribution.Artifacts.Add(AndroidArtifact);
            return contribution;
        }

        /// <summary>
        /// Creates one home-screen widget per configured campaign in the generated Gradle project.
        /// </summary>
        private static void StageAndroidWidgets(string path)
        {
            var own = WidgetsModule.Own();
            if (own == null)
            {
                throw new BuildFailedException(
                    "[Tapp] The Widgets package has no settings, so the Android widgets were not created.");
            }

            // With no id there is nothing to stage, and this still runs: it is the only place that removes what
            // an earlier build staged, when Unity hands back a Gradle project that build already edited.
            var widgets = own.ConfiguredWidgetIds().Select(id => new AndroidWidgetStaging.Widget(id)).ToList();
            var shared = new Dictionary<string, string>
            {
                ["__TAPP_WIDGET_LABEL__"] = AndroidWidgetStaging.ResourceString(
                    string.IsNullOrWhiteSpace(own.androidWidgetLabel) ? Application.productName : own.androidWidgetLabel),
                ["__TAPP_WIDGET_DESCRIPTION__"] = AndroidWidgetStaging.ResourceString(own.androidWidgetDescription),
                ["__TAPP_UPDATE_PERIOD_MILLIS__"] = own.androidWidgetUpdatePeriodMillis.ToString(CultureInfo.InvariantCulture),
            };

            var staged = AndroidWidgetStaging.StageWidgets(
                TappEditorModules.PackageFolder(typeof(WidgetsEditorModule).Assembly, AndroidTemplatePath),
                path, widgets, shared);

            var manifest = Path.Combine(path, "src", "main", "AndroidManifest.xml");
            if (!File.Exists(manifest))
            {
                if (widgets.Count == 0)
                {
                    // No receiver to declare and no file an earlier build could have left one in.
                    return;
                }

                throw new BuildFailedException($"[Tapp] {manifest} does not exist, so the widget receivers cannot be declared.");
            }

            AndroidWidgetStaging.MergeManifest(manifest, staged.ReceiverFragments);

            Debug.Log(widgets.Count == 0
                ? "[Tapp] No widget ids are configured, so no home-screen widget was staged into " +
                  $"{Path.GetFileName(path)}."
                : $"[Tapp] Staged {widgets.Count} widget(s) into {Path.GetFileName(path)}: " +
                  string.Join(", ", widgets.Select(widget => widget.ClassName)) + ".");
        }
    }
}
