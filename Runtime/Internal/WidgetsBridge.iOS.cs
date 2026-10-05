#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using TappGo.Modules;

namespace TappGo.Widgets.Internal
{
    /// <summary>
    /// The device implementation: the C entry points in <c>Runtime/Plugins/iOS/TappGoShim.Widgets.swift</c>.
    /// </summary>
    /// <remarks>
    /// Same members, same signatures and same envelopes as the Android body and the Editor stub beside it; the
    /// <c>#if</c> at the top of each decides which compiles. The ownership rules are <see cref="TappNative"/>'s.
    /// </remarks>
    internal static class WidgetsBridge
    {
        /// <summary>iOS configures the Widgets module from core's <c>configure</c> payload; there is no second call.</summary>
        internal static string Configure()
        {
            return null;
        }

        internal static void ValidateToken(string json, Action<string> completion)
        {
            TappGo_ValidateToken(TappNative.Encode(json), TappNative.Pin(completion), TappNative.Reply);
        }

        internal static string ShowNextEntry()
        {
            return TappNative.Take(TappGo_ShowNextWidgetEntry());
        }

        internal static string MoveToEntry(string json)
        {
            return TappNative.Take(TappGo_MoveToWidgetEntry(TappNative.Encode(json)));
        }

        internal static string SetText(string json)
        {
            return TappNative.Take(TappGo_SetWidgetText(TappNative.Encode(json)));
        }

        internal static string SetCountdownDuration(string json)
        {
            return TappNative.Take(TappGo_SetWidgetCountdownDuration(TappNative.Encode(json)));
        }

        internal static string IsInstalled(string json)
        {
            return TappNative.Take(TappGo_IsWidgetInstalled(TappNative.Encode(json)));
        }

        internal static string PendingLink()
        {
            return TappNative.Take(TappGo_PendingWidgetLink());
        }

        internal static void StartPictureInPicture(string json, Action<string> completion)
        {
            TappGo_StartPictureInPicture(TappNative.Encode(json), TappNative.Pin(completion), TappNative.Reply);
        }

        internal static void StopPictureInPicture(Action<string> completion)
        {
            TappGo_StopPictureInPicture(TappNative.Pin(completion), TappNative.Reply);
        }

        // MARK: - Pinning
        //
        // Android only. iOS places widgets from the Home Screen and nothing else, so these answer the neutral
        // value here rather than crossing into a framework that has no such member — the same shape native
        // gives below its OS floor. Constant envelopes, no logic, no new codes.

        internal static string IsPinSupported()
        {
            return "{\"ok\":true,\"value\":{\"supported\":false}}";
        }

        internal static string RequestPin(string json)
        {
            return "{\"ok\":true,\"value\":{\"requested\":false}}";
        }

        internal static string PlacedCount(string json)
        {
            return "{\"ok\":true,\"value\":{\"count\":0}}";
        }

        // Names match the C symbols exactly, so none of these needs an EntryPoint and a typo cannot silently
        // bind to the wrong function.

        [DllImport("__Internal")]
        private static extern void TappGo_ValidateToken(byte[] json, IntPtr context, TappNative.ReplyCallback reply);

        [DllImport("__Internal")]
        private static extern IntPtr TappGo_ShowNextWidgetEntry();

        [DllImport("__Internal")]
        private static extern IntPtr TappGo_MoveToWidgetEntry(byte[] json);

        [DllImport("__Internal")]
        private static extern IntPtr TappGo_SetWidgetText(byte[] json);

        [DllImport("__Internal")]
        private static extern IntPtr TappGo_SetWidgetCountdownDuration(byte[] json);

        [DllImport("__Internal")]
        private static extern IntPtr TappGo_IsWidgetInstalled(byte[] json);

        [DllImport("__Internal")]
        private static extern IntPtr TappGo_PendingWidgetLink();

        [DllImport("__Internal")]
        private static extern void TappGo_StartPictureInPicture(byte[] json, IntPtr context, TappNative.ReplyCallback reply);

        [DllImport("__Internal")]
        private static extern void TappGo_StopPictureInPicture(IntPtr context, TappNative.ReplyCallback reply);
    }
}
#endif
