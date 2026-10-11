#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using TappGo.Modules;

namespace TappGo.Widgets.Internal
{
    /// <summary>
    /// The Android implementation: <c>com.tappgo.sdk.bridge.TappWidgetsBridge</c> through JNI.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The widgets module has a bridge class of its own, in the <c>tapp-widgets</c> artifact. It is called
    /// directly, with no probe for whether it exists: installing this package is what puts that artifact in
    /// the build, so the class being absent is a packaging failure — and <see cref="TappNative.Call"/> reports
    /// it as one.
    /// </para>
    /// <para>
    /// Every member is on that class, <c>validateToken</c> included: a claim is the widget surface's on
    /// Android as it is on iOS, so the member left core's <c>TappBridge</c> in native 2.0.0 (contract
    /// revision 14, Tapp-Android D-135). The request and the reply did not change.
    /// </para>
    /// </remarks>
    internal static class WidgetsBridge
    {
        private const string BridgeClass = "com.tappgo.sdk.bridge.TappWidgetsBridge";

        /// <summary>
        /// The widgets module's own configure, which must follow core's.
        /// </summary>
        /// <remarks>
        /// Skipping it loses the check that core and widgets are the same version, and leaves
        /// <c>isWidgetInstalled</c> answering <c>false</c> for a widget that is plainly on the Home Screen.
        /// </remarks>
        internal static string Configure()
        {
            return TappNative.Call(BridgeClass, "configure");
        }

        internal static void ValidateToken(string json, Action<string> completion)
        {
            TappNative.CallAsync(BridgeClass, "validateToken", completion, json);
        }

        internal static string ShowNextEntry()
        {
            return TappNative.Call(BridgeClass, "showNextWidgetEntry");
        }

        internal static string MoveToEntry(string json)
        {
            return TappNative.Call(BridgeClass, "moveToWidgetEntry", json);
        }

        internal static string SetText(string json)
        {
            return TappNative.Call(BridgeClass, "setWidgetText", json);
        }

        internal static string SetCountdownDuration(string json)
        {
            return TappNative.Call(BridgeClass, "setWidgetCountdownDuration", json);
        }

        internal static string IsInstalled(string json)
        {
            return TappNative.Call(BridgeClass, "isWidgetInstalled", json);
        }

        internal static string PendingLink()
        {
            return TappNative.Call(BridgeClass, "pendingWidgetLink");
        }

        // MARK: - Picture in Picture
        //
        // iOS only. The code is the one the iOS SDK reports for a device that cannot present one.

        private const string NoPictureInPicture =
            "{\"ok\":false,\"code\":\"pictureInPictureUnsupported\",\"message\":\"Picture in Picture is iOS only.\"}";

        internal static void StartPictureInPicture(string json, Action<string> completion)
        {
            completion?.Invoke(NoPictureInPicture);
        }

        internal static void StopPictureInPicture(Action<string> completion)
        {
            completion?.Invoke("{\"ok\":true}");
        }

        // MARK: - Pinning

        internal static string IsPinSupported()
        {
            return TappNative.Call(BridgeClass, "isPinWidgetSupported");
        }

        internal static string RequestPin(string json)
        {
            return TappNative.Call(BridgeClass, "requestPinWidget", json);
        }

        internal static string PlacedCount(string json)
        {
            return TappNative.Call(BridgeClass, "placedWidgetCount", json);
        }
    }
}
#endif
