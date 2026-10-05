using System;
using TappGo.Modules;
using TappGo.Widgets.Internal;

namespace TappGo
{
    /// <summary>
    /// Tapp widgets: drive the home-screen widgets on both platforms, read the link one opened the app with,
    /// and — on iOS — play a video in Picture in Picture.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A widget is rendered natively — by the widget extension Tapp adds to your Xcode project, and by the
    /// home-screen widget it adds to your Gradle project. Nothing here draws anything: you name a widget and
    /// a screen, both configured server-side, and the SDK resolves the content.
    /// </para>
    /// <para>
    /// <b>Picture in Picture needs no widget.</b> With no widget id in the settings and Picture in Picture on,
    /// neither build adds a widget: <see cref="StartPictureInPicture"/>, <see cref="StopPictureInPicture"/>
    /// and <see cref="PendingLink"/> work as they always do, and <see cref="IsInstalled"/> with no argument and
    /// <see cref="ShowNextEntry"/> answer <c>false</c>. <see cref="MoveToEntry"/>, <see cref="SetText"/>,
    /// <see cref="SetCountdownDuration"/> and <see cref="IsInstalled"/> with a widget id answer an error —
    /// there is no such widget — and so do <see cref="RequestPin"/> and <see cref="PlacedCount"/> on Android.
    /// <see cref="ValidateToken"/> has no widget to speak for: on Android it answers <c>IsValid</c> false, and
    /// on iOS it asks the back office as it always does.
    /// </para>
    /// <para>
    /// <b>One API, two platforms.</b> Picture in Picture is iOS-only and answers
    /// <see cref="TappErrorCode.PictureInPictureUnsupported"/> on Android; pinning a widget from the app is
    /// Android-only and answers <c>false</c> on iOS. Neither is an error to guard against with <c>#if</c>.
    /// </para>
    /// <para>
    /// Configure the SDK first with <see cref="Tapp.Configure"/>; Tapp does nothing until it is called.
    /// <b>In the Editor every call is a no-op that logs</b>, and <b>callbacks arrive on the main thread</b>.
    /// </para>
    /// </remarks>
    public static class TappWidgets
    {
        /// <summary>
        /// Asks the Tapp back office whether a prize token is genuine, and for whom.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A token arrives inside the link a widget opens the app with — <see cref="PendingLink"/> says where
        /// that link arrives.
        /// Validate it before granting anything: the widget runs outside your process, and the link is the
        /// only thing crossing back.
        /// </para>
        /// <para>
        /// <b>An invalid token is a success with <see cref="TappTokenValidation.IsValid"/> false</b>, not a
        /// failure. A failure means the question could not be asked at all.
        /// </para>
        /// </remarks>
        /// <param name="token">The token as it appeared in the link.</param>
        /// <param name="widgetId">The widget the token was minted by.</param>
        /// <param name="completion">Called on the main thread with the server's answer.</param>
        public static void ValidateToken(string token, string widgetId, Action<TappResult<TappTokenValidation>> completion = null)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(widgetId))
            {
                TappEnvelope.DeliverLocal(completion, TappEnvelope.Rejected<TappTokenValidation>(TappErrorCode.InvalidRequest, "token and widgetId are both required."));
                return;
            }

            var json = new JsonBuilder().Add("token", token).Add("widgetID", widgetId).Build();
            WidgetsBridge.ValidateToken(json, reply => TappEnvelope.Deliver(completion, reply, WidgetReply.ParseTokenValidation));
        }

        /// <summary>
        /// Turns every widget to its next screen.
        /// </summary>
        /// <remarks>
        /// The screens and their order are configured server-side. <c>true</c> means a widget changed page;
        /// <c>false</c> means nothing did — a one-screen campaign, or no widget placed — and is a success.
        /// </remarks>
        public static TappResult<bool> ShowNextEntry()
        {
            return WidgetReply.ParseChanged(WidgetsBridge.ShowNextEntry());
        }

        /// <summary>
        /// Shows a specific screen of a widget.
        /// </summary>
        /// <param name="widgetId">The widget, as configured in Project Settings → Tapp.</param>
        /// <param name="entryId">The server-configured screen to show. Case is ignored.</param>
        /// <returns><c>true</c> if the widget changed page; <c>false</c> if it was already there or is not placed.</returns>
        public static TappResult<bool> MoveToEntry(string widgetId, string entryId)
        {
            if (string.IsNullOrEmpty(widgetId) || string.IsNullOrEmpty(entryId))
            {
                return TappEnvelope.Rejected<bool>(TappErrorCode.InvalidRequest, "widgetId and entryId are both required.");
            }

            var json = new JsonBuilder().Add("widgetID", widgetId).Add("entryID", entryId).Build();
            return WidgetReply.ParseChanged(WidgetsBridge.MoveToEntry(json));
        }

        /// <summary>
        /// Replaces the text of one node on one screen of a widget.
        /// </summary>
        /// <remarks>
        /// <b>The empty string clears the label</b> — it is a value, not an omission. Nodes are named in the
        /// design, server-side. <c>false</c> means the node was not found and is a success.
        /// </remarks>
        /// <param name="widgetId">The widget, as configured in Project Settings → Tapp.</param>
        /// <param name="entryId">The screen holding the node. Case is ignored.</param>
        /// <param name="nodeId">The node's name in the design.</param>
        /// <param name="text">The text to show. Never <c>null</c>.</param>
        public static TappResult<bool> SetText(string widgetId, string entryId, string nodeId, string text)
        {
            if (string.IsNullOrEmpty(widgetId) || string.IsNullOrEmpty(entryId) || string.IsNullOrEmpty(nodeId) || text == null)
            {
                return TappEnvelope.Rejected<bool>(TappErrorCode.InvalidRequest, "widgetId, entryId, nodeId and text are all required.");
            }

            var json = new JsonBuilder()
                .Add("widgetID", widgetId)
                .Add("entryID", entryId)
                .Add("nodeID", nodeId)
                .Add("text", text)
                .Build();
            return WidgetReply.ParseApplied(WidgetsBridge.SetText(json));
        }

        /// <summary>
        /// Restarts a countdown node on one screen of a widget.
        /// </summary>
        /// <param name="widgetId">The widget, as configured in Project Settings → Tapp.</param>
        /// <param name="entryId">The screen holding the node. Case is ignored.</param>
        /// <param name="nodeId">The countdown node's name in the design.</param>
        /// <param name="seconds">How long it runs, in whole seconds from <b>now</b>. Zero shows it finished.</param>
        /// <returns><c>true</c> if the node was found and set; <c>false</c> if it was not, which is a success.</returns>
        public static TappResult<bool> SetCountdownDuration(string widgetId, string entryId, string nodeId, int seconds)
        {
            if (string.IsNullOrEmpty(widgetId) || string.IsNullOrEmpty(entryId) || string.IsNullOrEmpty(nodeId))
            {
                return TappEnvelope.Rejected<bool>(TappErrorCode.InvalidRequest, "widgetId, entryId and nodeId are all required.");
            }

            var json = new JsonBuilder()
                .Add("widgetID", widgetId)
                .Add("entryID", entryId)
                .Add("nodeID", nodeId)
                .AddInt("seconds", seconds)
                .Build();
            return WidgetReply.ParseApplied(WidgetsBridge.SetCountdownDuration(json));
        }

        /// <summary>
        /// Whether the player has placed a Tapp widget on their Home Screen.
        /// </summary>
        /// <param name="widgetId">
        /// One widget, or <c>null</c> (the default) for "any widget this app declares" — the ordinary question
        /// for an app with one campaign.
        /// </param>
        public static TappResult<bool> IsInstalled(string widgetId = null)
        {
            var json = new JsonBuilder().Add("widgetID", string.IsNullOrEmpty(widgetId) ? null : widgetId).Build();
            return WidgetReply.ParseInstalled(WidgetsBridge.IsInstalled(json));
        }

        /// <summary>
        /// The link a widget opened the app with, once — or <c>null</c> when none is waiting.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A consume-once inbox: the first read returns the link and clears it, so read it where you act on it.
        /// Check it at startup and whenever the app comes back to the foreground — a widget tap brings the app
        /// forward, and the link is what says why.
        /// </para>
        /// <para>
        /// <b>The same call on both platforms.</b> On iOS the SDK parks the link when the widget opens the app;
        /// on Android the SDK parks it before launching the app. A parked link needs none of your deep-link
        /// routing.
        /// </para>
        /// <para>
        /// <b>One case on iOS is not parked.</b> A <i>widget tap</i> whose link carries a URL scheme is opened by
        /// iOS itself, so it arrives through <c>Application.deepLinkActivated</c> (and
        /// <c>Application.absoluteURL</c> on a cold start) and never appears here. A widget link with no scheme
        /// waits here, and on Android every widget link does. A Picture in Picture restore link always waits
        /// here, whatever its scheme.
        /// </para>
        /// </remarks>
        public static TappResult<string> PendingLink()
        {
            return WidgetReply.ParseLink(WidgetsBridge.PendingLink());
        }

        // MARK: - Picture in Picture (iOS)

        /// <summary>
        /// Plays a video in a floating window that stays up while the player leaves the app. iOS only.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Needs <see cref="TappWidgetsSettings.pictureInPictureEnabled"/> on, which is what puts <c>audio</c> under
        /// the app's <c>UIBackgroundModes</c>; without it every start fails with
        /// <see cref="TappErrorCode.PictureInPictureBackgroundAudioMissing"/>. On Android, and on an iOS device
        /// that cannot present one, the answer is <see cref="TappErrorCode.PictureInPictureUnsupported"/> —
        /// expected, not exceptional.
        /// </para>
        /// <para>
        /// It needs no widget id: the settings' list may be empty when this is all the package is installed
        /// for. It does need <see cref="Tapp.Configure"/> to have succeeded, and so an App Group.
        /// </para>
        /// </remarks>
        /// <param name="request">What to play, and what link to leave behind.</param>
        /// <param name="completion">Called on the main thread once the window is up, or with why it isn't.</param>
        public static void StartPictureInPicture(TappPictureInPictureRequest request, Action<TappResult> completion = null)
        {
            if (string.IsNullOrEmpty(request.VideoUrl))
            {
                TappEnvelope.DeliverLocal(completion, TappEnvelope.Rejected(TappErrorCode.InvalidRequest, "VideoUrl is required."));
                return;
            }

            var json = new JsonBuilder()
                .Add("videoURL", request.VideoUrl)
                .Add("deepLink", string.IsNullOrEmpty(request.DeepLink) ? null : request.DeepLink)
                .Build();
            WidgetsBridge.StartPictureInPicture(json, reply => TappEnvelope.Deliver(completion, reply, TappEnvelope.Parse));
        }

        /// <summary>
        /// Closes the Picture in Picture window. Succeeds when there is none.
        /// </summary>
        /// <param name="completion">Called on the main thread.</param>
        public static void StopPictureInPicture(Action<TappResult> completion = null)
        {
            WidgetsBridge.StopPictureInPicture(reply => TappEnvelope.Deliver(completion, reply, TappEnvelope.Parse));
        }

        // MARK: - Pinning (Android)

        /// <summary>
        /// Whether this launcher lets an app ask to place a widget. Android only; always <c>false</c> on iOS.
        /// </summary>
        public static TappResult<bool> IsPinSupported()
        {
            return WidgetReply.ParseSupported(WidgetsBridge.IsPinSupported());
        }

        /// <summary>
        /// Asks the launcher to offer the player a widget to place. Android only.
        /// </summary>
        /// <remarks>
        /// The launcher shows its own confirmation; <c>true</c> means it was shown, not that the player
        /// accepted. Watch <see cref="PlacedCount"/> or <see cref="IsInstalled"/> for that. On iOS
        /// the answer is <c>false</c>: widgets are placed from the Home Screen only.
        /// </remarks>
        /// <param name="widgetId">Which of the widgets in Project Settings → Tapp to offer.</param>
        public static TappResult<bool> RequestPin(string widgetId)
        {
            if (string.IsNullOrEmpty(widgetId))
            {
                return TappEnvelope.Rejected<bool>(TappErrorCode.InvalidRequest, "widgetId is required.");
            }

            var json = new JsonBuilder().Add("provider", AndroidWidgetNaming.QualifiedName(widgetId)).Build();
            return WidgetReply.ParseRequested(WidgetsBridge.RequestPin(json));
        }

        /// <summary>
        /// How many copies of a widget the player has placed. Android only; always <c>0</c> on iOS.
        /// </summary>
        /// <param name="widgetId">Which of the widgets in Project Settings → Tapp to count.</param>
        public static TappResult<int> PlacedCount(string widgetId)
        {
            if (string.IsNullOrEmpty(widgetId))
            {
                return TappEnvelope.Rejected<int>(TappErrorCode.InvalidRequest, "widgetId is required.");
            }

            var json = new JsonBuilder().Add("provider", AndroidWidgetNaming.QualifiedName(widgetId)).Build();
            return WidgetReply.ParseCount(WidgetsBridge.PlacedCount(json));
        }
    }
}
