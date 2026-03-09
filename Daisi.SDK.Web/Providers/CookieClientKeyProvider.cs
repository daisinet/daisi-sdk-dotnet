using Daisi.SDK.Interfaces.Authentication;
using Daisi.SDK.Web.Services;
using Microsoft.AspNetCore.Http;

namespace Daisi.SDK.Web.Providers
{
    /// <summary>
    /// Reads the client key from the HTTP cookie on first access and caches it for the
    /// lifetime of the scope. This is critical for Blazor Server circuits where
    /// <see cref="IHttpContextAccessor.HttpContext"/> is only available during the initial
    /// SSR render but not during subsequent interactive (SignalR) callbacks.
    /// </summary>
    public class CookieClientKeyProvider(IHttpContextAccessor httpContextAccessor) : IClientKeyProvider
    {
        private string? _cachedKey;

        public string GetClientKey()
        {
            if (_cachedKey != null)
                return _cachedKey;

            var key = httpContextAccessor.HttpContext?.Request.Cookies[AuthService.CLIENT_KEY_STORAGE_KEY];
            if (!string.IsNullOrEmpty(key))
                _cachedKey = key;

            return key ?? string.Empty;
        }
    }
}
