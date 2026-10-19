#if UNITY_EDITOR || (!UNITY_IOS && !UNITY_ANDROID)
using System;
using TappGo.Modules;

namespace TappGo.Widgets.Internal
{
    /// <summary>
    /// The Editor stub: logs what would have been sent, and answers. See <see cref="TappNative"/> for what a
    /// stub may and may not claim.
    /// </summary>
    /// <remarks>
    /// Every answer is the <i>neutral</i> one — nothing changed, nothing applied, nothing placed, nothing
    /// waiting, nothing valid. A stub that reported a widget installed would let a host's "prompt the player to
    /// add it" flow look finished in Play mode.
    /// </remarks>
    internal static class WidgetsBridge
    {
        internal static string Configure()
        {
            return null;
        }

        internal static void ValidateToken(string json, Action<string> completion)
        {
            TappNative.Log("ValidateToken", json);
            completion?.Invoke("{\"ok\":true,\"value\":{\"isValid\":false}}");
        }

        internal static string ShowNextEntry()
        {
            TappNative.Log("ShowNextWidgetEntry");
            return "{\"ok\":true,\"value\":{\"changed\":false}}";
        }

        internal static string MoveToEntry(string json)
        {
            TappNative.Log("MoveToWidgetEntry", json);
            return "{\"ok\":true,\"value\":{\"changed\":false}}";
        }

        internal static string SetText(string json)
        {
            TappNative.Log("SetWidgetText", json);
            return "{\"ok\":true,\"value\":{\"applied\":false}}";
        }

        internal static string SetCountdownDuration(string json)
        {
            TappNative.Log("SetWidgetCountdownDuration", json);
            return "{\"ok\":true,\"value\":{\"applied\":false}}";
        }

        internal static string IsInstalled(string json)
        {
            TappNative.Log("IsWidgetInstalled", json);
            return "{\"ok\":true,\"value\":{\"installed\":false}}";
        }

        internal static string PendingLink()
        {
            TappNative.Log("PendingWidgetLink");
            return "{\"ok\":true,\"value\":{}}";
        }

        internal static void StartPictureInPicture(string json, Action<string> completion)
        {
            TappNative.Log("StartPictureInPicture", json);
            completion?.Invoke(TappNative.Ok);
        }

        internal static void StopPictureInPicture(Action<string> completion)
        {
            TappNative.Log("StopPictureInPicture");
            completion?.Invoke(TappNative.Ok);
        }

        internal static string IsPinSupported()
        {
            TappNative.Log("IsPinWidgetSupported");
            return "{\"ok\":true,\"value\":{\"supported\":false}}";
        }

        internal static string RequestPin(string json)
        {
            TappNative.Log("RequestPinWidget", json);
            return "{\"ok\":true,\"value\":{\"requested\":false}}";
        }

        internal static string PlacedCount(string json)
        {
            TappNative.Log("PlacedWidgetCount", json);
            return "{\"ok\":true,\"value\":{\"count\":0}}";
        }
    }
}
#endif
