using System;
using TappGo.Modules;

namespace TappGo.Widgets.Internal
{
    /// <summary>
    /// Reads the widget and Picture in Picture replies. The envelope rules every Tapp reply shares are
    /// <see cref="TappEnvelope"/>'s; what is here is the shape of each <c>value</c>.
    /// </summary>
    internal static class WidgetReply
    {
        /// <summary><c>{"value":{"changed":true}}</c> — show-next and move-to.</summary>
        internal static TappResult<bool> ParseChanged(string json)
        {
            var envelope = TappEnvelope.Decode<ChangedEnvelope>(json);
            return envelope == null
                ? TappEnvelope.Unreadable<bool>(json)
                : TappEnvelope.Flag(envelope.ok, envelope.value != null && envelope.value.changed, envelope.code, envelope.message);
        }

        /// <summary><c>{"value":{"applied":true}}</c> — set-text and set-countdown.</summary>
        internal static TappResult<bool> ParseApplied(string json)
        {
            var envelope = TappEnvelope.Decode<AppliedEnvelope>(json);
            return envelope == null
                ? TappEnvelope.Unreadable<bool>(json)
                : TappEnvelope.Flag(envelope.ok, envelope.value != null && envelope.value.applied, envelope.code, envelope.message);
        }

        /// <summary><c>{"value":{"installed":true}}</c>.</summary>
        internal static TappResult<bool> ParseInstalled(string json)
        {
            var envelope = TappEnvelope.Decode<InstalledEnvelope>(json);
            return envelope == null
                ? TappEnvelope.Unreadable<bool>(json)
                : TappEnvelope.Flag(envelope.ok, envelope.value != null && envelope.value.installed, envelope.code, envelope.message);
        }

        /// <summary><c>{"value":{"supported":true}}</c>.</summary>
        internal static TappResult<bool> ParseSupported(string json)
        {
            var envelope = TappEnvelope.Decode<SupportedEnvelope>(json);
            return envelope == null
                ? TappEnvelope.Unreadable<bool>(json)
                : TappEnvelope.Flag(envelope.ok, envelope.value != null && envelope.value.supported, envelope.code, envelope.message);
        }

        /// <summary><c>{"value":{"requested":true}}</c>.</summary>
        internal static TappResult<bool> ParseRequested(string json)
        {
            var envelope = TappEnvelope.Decode<RequestedEnvelope>(json);
            return envelope == null
                ? TappEnvelope.Unreadable<bool>(json)
                : TappEnvelope.Flag(envelope.ok, envelope.value != null && envelope.value.requested, envelope.code, envelope.message);
        }

        /// <summary><c>{"value":{"count":2}}</c>.</summary>
        internal static TappResult<int> ParseCount(string json)
        {
            var envelope = TappEnvelope.Decode<CountEnvelope>(json);
            if (envelope == null)
            {
                return TappEnvelope.Unreadable<int>(json);
            }

            return envelope.ok
                ? TappEnvelope.Success(envelope.value?.count ?? 0)
                : TappEnvelope.Failure<int>(envelope.code, envelope.message);
        }

        /// <summary>
        /// <c>{"value":{"link":"…"}}</c>, or <c>{"value":{}}</c> when nothing is waiting.
        /// </summary>
        /// <remarks>
        /// The key is <i>omitted</i> for "none", never written as <c>null</c>, and <c>JsonUtility</c> reads an
        /// absent string as <c>""</c> — normalised to <c>null</c>, because the empty string is not a link a host
        /// could open and testing for both would be every caller's job otherwise.
        /// </remarks>
        internal static TappResult<string> ParseLink(string json)
        {
            var envelope = TappEnvelope.Decode<LinkEnvelope>(json);
            if (envelope == null)
            {
                return TappEnvelope.Unreadable<string>(json);
            }

            if (!envelope.ok)
            {
                return TappEnvelope.Failure<string>(envelope.code, envelope.message);
            }

            var link = envelope.value?.link;
            return TappEnvelope.Success(string.IsNullOrEmpty(link) ? null : link);
        }

        /// <summary><c>{"value":{"isValid":true,"prize":"…","userID":"…"}}</c> — prize and user omitted when invalid.</summary>
        internal static TappResult<TappTokenValidation> ParseTokenValidation(string json)
        {
            var envelope = TappEnvelope.Decode<TokenValidationEnvelope>(json);
            if (envelope == null)
            {
                return TappEnvelope.Unreadable<TappTokenValidation>(json);
            }

            if (!envelope.ok)
            {
                return TappEnvelope.Failure<TappTokenValidation>(envelope.code, envelope.message);
            }

            var dto = envelope.value;
            var validation = new TappTokenValidation(
                dto != null && dto.isValid,
                string.IsNullOrEmpty(dto?.prize) ? null : dto.prize,
                string.IsNullOrEmpty(dto?.userID) ? null : dto.userID);
            return TappEnvelope.Success(validation);
        }

        // Wire shapes. Field names match docs/NATIVE-CONTRACT.md exactly and are never renamed.
        //
        // One envelope per `value` key. JsonUtility binds by field name, so a shared "flag" DTO would need
        // the key to be the same on every call — and the contract deliberately names each answer after the
        // question (`changed`, `applied`, `installed`), so a reply pasted into the wrong parser reads as false
        // rather than as true.

        [Serializable]
        private class ChangedEnvelope
        {
            public bool ok;
            public string code;
            public string message;
            public ChangedDto value;
        }

        [Serializable]
        private class AppliedEnvelope
        {
            public bool ok;
            public string code;
            public string message;
            public AppliedDto value;
        }

        [Serializable]
        private class InstalledEnvelope
        {
            public bool ok;
            public string code;
            public string message;
            public InstalledDto value;
        }

        [Serializable]
        private class SupportedEnvelope
        {
            public bool ok;
            public string code;
            public string message;
            public SupportedDto value;
        }

        [Serializable]
        private class RequestedEnvelope
        {
            public bool ok;
            public string code;
            public string message;
            public RequestedDto value;
        }

        [Serializable]
        private class CountEnvelope
        {
            public bool ok;
            public string code;
            public string message;
            public CountDto value;
        }

        [Serializable]
        private class LinkEnvelope
        {
            public bool ok;
            public string code;
            public string message;
            public LinkDto value;
        }

        [Serializable]
        private class TokenValidationEnvelope
        {
            public bool ok;
            public string code;
            public string message;
            public TokenValidationDto value;
        }

        [Serializable]
        private class ChangedDto
        {
            public bool changed;
        }

        [Serializable]
        private class AppliedDto
        {
            public bool applied;
        }

        [Serializable]
        private class InstalledDto
        {
            public bool installed;
        }

        [Serializable]
        private class SupportedDto
        {
            public bool supported;
        }

        [Serializable]
        private class RequestedDto
        {
            public bool requested;
        }

        [Serializable]
        private class CountDto
        {
            public int count;
        }

        [Serializable]
        private class LinkDto
        {
            public string link;
        }

        [Serializable]
        private class TokenValidationDto
        {
            public bool isValid;
            public string prize;
            public string userID;
        }
    }
}
