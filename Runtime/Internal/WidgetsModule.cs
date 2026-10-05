using TappGo.Modules;
using UnityEngine;

namespace TappGo.Widgets.Internal
{
    /// <summary>
    /// What core asks this package at <c>configure</c>: whether its settings are usable, its keys, and — on
    /// Android — the widgets module's own configure call.
    /// </summary>
    internal sealed class WidgetsModule : TappModule
    {
        internal const string PackageName = "com.tapp.go.widgets";

        public override string Id => PackageName;

        /// <summary>
        /// Stands in for the asset in <c>Resources</c>. A test sets it; nothing else does, so the running app
        /// and the build always read the one asset the developer edits.
        /// </summary>
        internal static TappWidgetsSettings Override;

        internal static TappWidgetsSettings Own()
        {
            return Override != null ? Override : TappWidgetsSettings.Load();
        }

        /// <summary>
        /// The package has to have something to do: at least one well-formed widget id, or Picture in Picture
        /// switched on.
        /// </summary>
        /// <remarks>
        /// <para>
        /// With neither, a host installed this package and gets nothing from it — an empty iOS gallery, no
        /// Android provider, no window — and the place to learn that is here rather than on a device. With
        /// Picture in Picture on and no ids, the package is installed for the window alone, which needs no
        /// widget id on either native SDK: the builds link the module and stage no widget.
        /// </para>
        /// <para>
        /// The same answer on both platforms, though Picture in Picture is iOS-only. One settings asset serves
        /// both builds, so refusing the Android one would make a cross-platform game invent a widget id it
        /// does not have; there the calls answer <c>pictureInPictureUnsupported</c> and nothing is staged.
        /// </para>
        /// </remarks>
        public override string Validate(TappGoSettings settings, TappPlatform platform)
        {
            var own = Own();
            if (own == null)
            {
                return "The Widgets package is installed but has no settings. Open Project Settings → Tapp and " +
                       "create them.";
            }

            var malformed = own.WidgetIdsProblem();
            if (malformed != null)
            {
                return malformed;
            }

            var widgets = own.ConfiguredWidgetIds().Length;
            if (widgets == 0 && !own.pictureInPictureEnabled)
            {
                return "widgetIds is empty and Picture in Picture is off, so the Widgets package has nothing to " +
                       "do. In Project Settings → Tapp → Widgets, add a widget id — or turn on Picture in " +
                       "Picture, if that is all you use this package for.";
            }

            // iOS only. The widget extension declares a fixed number of slots, and native refuses a longer list
            // when the extension configures itself — an error the generated bundle swallows on purpose, so the
            // build would pass and every widget render its placeholder on a device, with nothing logged.
            // Android creates a provider per id and has no such limit.
            if (platform == TappPlatform.iOS && widgets > TappWidgetsSettings.MaximumIosWidgetIds)
            {
                return $"widgetIds lists {widgets} widgets; the iOS widget extension serves at most " +
                       $"{TappWidgetsSettings.MaximumIosWidgetIds}. Shorten the list in Project Settings → Tapp → " +
                       "Widgets.";
            }

            // Judged only when a widget is staged: the period is written into the widget's description and
            // nowhere else, so with no widget it is a number nothing reads.
            if (widgets > 0 &&
                platform == TappPlatform.Android &&
                own.androidWidgetUpdatePeriodMillis < TappWidgetsSettings.MinimumAndroidUpdatePeriodMillis)
            {
                return $"androidWidgetUpdatePeriodMillis is {own.androidWidgetUpdatePeriodMillis}; Android " +
                       $"rounds anything under {TappWidgetsSettings.MinimumAndroidUpdatePeriodMillis} (30 minutes) " +
                       "up to it, so a smaller value only misleads. Set it in Project Settings → Tapp → Widgets.";
            }

            return null;
        }

        public override void ContributeConfiguration(JsonBuilder json, TappGoSettings settings, TappPlatform platform)
        {
            var own = Own();
            if (own != null)
            {
                json.AddStrings("widgetIDs", own.ConfiguredWidgetIds());
            }
        }

        public override TappResult? AfterConfigure(TappGoSettings settings, TappPlatform platform)
        {
            var reply = WidgetsBridge.Configure();
            return reply == null ? (TappResult?)null : TappEnvelope.Parse(reply);
        }

        /// <summary>
        /// Before any scene loads, so the module is in place before the host's first <c>Tapp.Configure</c> —
        /// the first thing to ask the registry.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Register()
        {
            TappModules.Register(new WidgetsModule());
        }
    }
}
