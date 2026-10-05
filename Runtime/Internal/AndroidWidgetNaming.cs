namespace TappGo.Widgets.Internal
{
    /// <summary>
    /// How the Android widget provider staged for a campaign is named — shared by the build hook that writes
    /// the Java class and the runtime call that names it to the SDK.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Android needs one <c>AppWidgetProvider</c> subclass per widget, and the SDK's pin-widget calls take the
    /// provider's class. Deriving the name from the widget id, rather than from its position in the settings,
    /// is what lets <see cref="TappWidgets.RequestPin"/> name it without remembering anything: the id the host
    /// passes is the id the class was staged for.
    /// </para>
    /// <para>
    /// Changing this after a release orphans the widget the player already placed — the launcher keeps a
    /// component name, and a renamed class is a different component.
    /// </para>
    /// </remarks>
    internal static class AndroidWidgetNaming
    {
        /// <summary>The Java package every staged provider lives in.</summary>
        internal const string Package = "com.tappgo.unity";

        /// <summary>The simple class name for a widget id: <c>TappWidgetProvider_a9bd63a1_2849_…</c>.</summary>
        internal static string ClassName(string widgetId)
        {
            return "TappWidgetProvider_" + (widgetId ?? "").Replace('-', '_');
        }

        /// <summary>The fully qualified name the SDK resolves with <c>Class.forName</c>.</summary>
        internal static string QualifiedName(string widgetId)
        {
            return Package + "." + ClassName(widgetId);
        }
    }
}
