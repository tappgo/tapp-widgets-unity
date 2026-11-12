namespace TappGo
{
    /// <summary>
    /// What the Tapp back office said about a prize token, as returned by <see cref="TappWidgets.ValidateToken"/>.
    /// </summary>
    /// <remarks>
    /// A token is the string a widget hands the app when the player taps a claim — usually inside the link
    /// <see cref="TappWidgets.PendingLink"/> returns. Validating it is the server's decision; this only carries
    /// the answer. An invalid token is <b>not an error</b>: <see cref="IsValid"/> is false and the rest is
    /// <c>null</c>.
    /// </remarks>
    public readonly struct TappTokenValidation
    {
        /// <summary>Whether the server accepted the token.</summary>
        public bool IsValid { get; }

        /// <summary>The prize the token was minted for, or <c>null</c> when invalid or when it named none.</summary>
        public string Prize { get; }

        /// <summary>The player the token was minted for, or <c>null</c> when invalid or when the server did not say.</summary>
        public string UserId { get; }

        internal TappTokenValidation(bool isValid, string prize, string userId)
        {
            IsValid = isValid;
            Prize = prize;
            UserId = userId;
        }

        /// <summary>A one-line form for logging.</summary>
        public override string ToString() => IsValid ? $"valid (prize: {Prize ?? "-"}, user: {UserId ?? "-"})" : "invalid";
    }

    /// <summary>
    /// What to play when starting Picture in Picture with <see cref="TappWidgets.StartPictureInPicture"/>.
    /// </summary>
    /// <remarks>
    /// iOS only. On Android every start answers <see cref="TappErrorCode.PictureInPictureUnsupported"/>.
    /// </remarks>
    public struct TappPictureInPictureRequest
    {
        /// <summary>The video to play, as an absolute <c>https</c> URL. Required.</summary>
        public string VideoUrl;

        /// <summary>
        /// The link to hand back through <see cref="TappWidgets.PendingLink"/> when the player taps the
        /// window to return to the app, or <c>null</c> for none.
        /// </summary>
        public string DeepLink;
    }
}
