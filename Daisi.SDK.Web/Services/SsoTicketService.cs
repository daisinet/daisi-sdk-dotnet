using Daisi.SDK.Models;
using System;
using System.Security.Cryptography;
using System.Text.Json;

namespace Daisi.SDK.Web.Services
{
    /// <summary>
    /// Stateless service that creates and validates short-lived AES-256-GCM encrypted SSO tickets.
    /// Tickets carry a <c>clientKey</c> and user metadata between SSO-participating apps.
    /// Register as a singleton via <see cref="Extensions.WebStartupExtensions.AddDaisiForWeb"/>.
    /// </summary>
    public class SsoTicketService
    {
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private const int TicketLifetimeSeconds = 60;

        /// <summary>
        /// Creates an encrypted SSO ticket containing the supplied user session data.
        /// The ticket is AES-256-GCM encrypted, base64url-encoded, and valid for 60 seconds.
        /// </summary>
        /// <param name="clientKey">The authenticated user's client key.</param>
        /// <param name="keyExpiration">When the client key expires.</param>
        /// <param name="userName">Display name of the user.</param>
        /// <param name="userRole">Role of the user (e.g. "User", "Manager", "Owner").</param>
        /// <param name="accountName">Name of the user's account.</param>
        /// <param name="accountId">ID of the user's account.</param>
        /// <returns>A base64url-encoded encrypted ticket string.</returns>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="DaisiStaticSettings.SsoSigningKey"/> is not configured.</exception>
        public string CreateTicket(string clientKey, string keyExpiration, string userName, string userRole, string accountName, string accountId)
        {
            var signingKey = DaisiStaticSettings.SsoSigningKey
                ?? throw new InvalidOperationException("SsoSigningKey is not configured.");

            var payload = new SsoTicketPayload
            {
                ClientKey = clientKey,
                KeyExpiration = keyExpiration,
                UserName = userName,
                UserRole = userRole,
                AccountName = accountName,
                AccountId = accountId,
                IssuedAtUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            var plaintext = JsonSerializer.SerializeToUtf8Bytes(payload);
            var key = Convert.FromBase64String(signingKey);

            var nonce = new byte[NonceSize];
            RandomNumberGenerator.Fill(nonce);

            var ciphertext = new byte[plaintext.Length];
            var tag = new byte[TagSize];

            using var aes = new AesGcm(key, TagSize);
            aes.Encrypt(nonce, plaintext, ciphertext, tag);

            // Wire format: nonce(12) + tag(16) + ciphertext
            var result = new byte[NonceSize + TagSize + ciphertext.Length];
            nonce.CopyTo(result, 0);
            tag.CopyTo(result, NonceSize);
            ciphertext.CopyTo(result, NonceSize + TagSize);

            return Base64UrlEncode(result);
        }

        /// <summary>
        /// Decrypts and validates an SSO ticket. Returns the payload if the ticket is valid
        /// and not expired, or <c>null</c> if the ticket is tampered, malformed, or expired.
        /// </summary>
        /// <param name="ticket">The base64url-encoded ticket string from the query parameter.</param>
        /// <returns>The decrypted <see cref="SsoTicketPayload"/>, or <c>null</c> if invalid.</returns>
        public SsoTicketPayload? ValidateTicket(string ticket)
        {
            try
            {
                var signingKey = DaisiStaticSettings.SsoSigningKey;
                if (string.IsNullOrEmpty(signingKey)) return null;

                var data = Base64UrlDecode(ticket);
                if (data.Length < NonceSize + TagSize) return null;

                var key = Convert.FromBase64String(signingKey);
                var nonce = data.AsSpan(0, NonceSize);
                var tag = data.AsSpan(NonceSize, TagSize);
                var ciphertext = data.AsSpan(NonceSize + TagSize);

                var plaintext = new byte[ciphertext.Length];

                using var aes = new AesGcm(key, TagSize);
                aes.Decrypt(nonce, ciphertext, tag, plaintext);

                var payload = JsonSerializer.Deserialize<SsoTicketPayload>(plaintext);
                if (payload is null) return null;

                var issuedAt = DateTimeOffset.FromUnixTimeSeconds(payload.IssuedAtUtc);
                if (DateTimeOffset.UtcNow - issuedAt > TimeSpan.FromSeconds(TicketLifetimeSeconds))
                    return null;

                return payload;
            }
            catch
            {
                return null;
            }
        }

        private static string Base64UrlEncode(byte[] data)
        {
            return Convert.ToBase64String(data)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static byte[] Base64UrlDecode(string s)
        {
            s = s.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
            }
            return Convert.FromBase64String(s);
        }
    }

    /// <summary>
    /// Payload carried inside an encrypted SSO ticket.
    /// Contains the user's session data needed to establish authentication on the relying-party app.
    /// </summary>
    public class SsoTicketPayload
    {
        /// <summary>The authenticated user's client key.</summary>
        public string ClientKey { get; set; } = "";

        /// <summary>When the client key expires.</summary>
        public string KeyExpiration { get; set; } = "";

        /// <summary>Display name of the user.</summary>
        public string UserName { get; set; } = "";

        /// <summary>Role of the user (e.g. "User", "Manager", "Owner").</summary>
        public string UserRole { get; set; } = "";

        /// <summary>Name of the user's account.</summary>
        public string AccountName { get; set; } = "";

        /// <summary>ID of the user's account.</summary>
        public string AccountId { get; set; } = "";

        /// <summary>Unix timestamp (seconds) when this ticket was issued.</summary>
        public long IssuedAtUtc { get; set; }
    }
}
