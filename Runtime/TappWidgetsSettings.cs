using UnityEngine;

namespace TappGo
{
    /// <summary>
    /// The Widgets settings, edited in <b>Project Settings → Tapp</b> beside core's.
    /// </summary>
    /// <remarks>
    /// Stored at <c>Assets/Resources/TappWidgetsSettings.asset</c> — in your project, not in the package, so it
    /// survives upgrades, and in an asset of its own so this package can be added and removed without touching
    /// core's. Every field has a default for the same reason core's do.
    /// </remarks>
    public sealed class TappWidgetsSettings : ScriptableObject
    {
        /// <summary>Where the asset lives, and the name <c>Resources.Load</c> looks for.</summary>
        internal const string ResourceName = "TappWidgetsSettings";

        [Tooltip("The Tapp widget ids this app renders, one per campaign, from the Tapp back office. Each is a " +
                 "UUID (8-4-4-4-12).\n\n" +
                 "Shared by both platforms: on iOS they configure the widget extension, on Android each one " +
                 "becomes a home-screen widget the build hook creates.\n\n" +
                 "The ORDER is a contract on iOS: WidgetKit knows a placed widget by its position in this " +
                 "list, so reordering swaps campaigns under widgets a player already placed. Append; never " +
                 "reorder. iOS serves at most 8 widgets; Android has no such limit.\n\n" +
                 "May be left empty only when Picture in Picture is on and is all you use this package for: " +
                 "the builds then add no widget and no widget extension for it.")]
        public string[] widgetIds = new string[0];

        [Header("Picture in Picture (iOS)")]
        [Tooltip("Let TappWidgets.StartPictureInPicture play a video in a floating window while the player " +
                 "leaves the app. Adds `audio` to UIBackgroundModes in the Xcode project — App Review asks " +
                 "why, so leave this off unless you use it.\n\n" +
                 "It needs no widget ids: with this on, the list above may stay empty.")]
        public bool pictureInPictureEnabled = false;

        [Header("Android")]
        [Tooltip("The name the launcher shows for the widget in its picker. Defaults to your product name.")]
        public string androidWidgetLabel = "";

        [Tooltip("The one-line description the launcher shows under the widget on Android 12 and later.")]
        public string androidWidgetDescription = "";

        [Tooltip("How often Android asks the widget to refresh, in milliseconds. The platform rounds anything " +
                 "under 30 minutes up to 30 minutes, so that is both the default and the floor.")]
        public int androidWidgetUpdatePeriodMillis = 1800000;

        /// <summary>The single settings asset, or <c>null</c> if none exists.</summary>
        public static TappWidgetsSettings Load()
        {
            return Resources.Load<TappWidgetsSettings>(ResourceName);
        }

        /// <summary>The floor Android applies to <c>updatePeriodMillis</c>: thirty minutes.</summary>
        internal const int MinimumAndroidUpdatePeriodMillis = 1800000;

        /// <summary>
        /// The most widget ids one iOS widget extension serves: native's <c>WidgetSlots.capacity</c>
        /// (TappWidgets-iOS). A rule of the pinned native SDK restated here, not one of this package's own —
        /// when the pin moves, check it still says eight.
        /// </summary>
        internal const int MaximumIosWidgetIds = 8;

        /// <summary>
        /// The widget ids with blanks dropped — what both platforms are configured with.
        /// </summary>
        /// <remarks>
        /// A blank row is what the Inspector leaves behind when a host adds an element and never fills it, and
        /// treating that as "a widget whose id is the empty string" would fail every launch for a stray click.
        /// </remarks>
        internal string[] ConfiguredWidgetIds()
        {
            if (widgetIds == null)
            {
                return new string[0];
            }

            var kept = new System.Collections.Generic.List<string>(widgetIds.Length);
            foreach (var id in widgetIds)
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    kept.Add(id.Trim());
                }
            }

            return kept.ToArray();
        }

        /// <summary>
        /// A widget id that is not a UUID, or a duplicate, explained — or <c>null</c> when all are usable.
        /// </summary>
        /// <remarks>
        /// Checked here rather than left to native because both SDKs refuse a malformed id at
        /// <c>configure</c> and the Android manifest carries it as a literal — a typo would surface as an
        /// opaque failure on a device rather than as this line in the Editor.
        /// </remarks>
        internal string WidgetIdsProblem()
        {
            var ids = ConfiguredWidgetIds();
            var seen = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var id in ids)
            {
                if (!System.Guid.TryParseExact(id, "D", out _))
                {
                    return $"widgetIds contains \"{id}\", which is not a UUID (8-4-4-4-12 hex). " +
                           "Copy the id from the Tapp back office into Project Settings → Tapp.";
                }

                if (!seen.Add(id))
                {
                    return $"widgetIds lists \"{id}\" twice. Each campaign appears once.";
                }
            }

            return null;
        }
    }
}
