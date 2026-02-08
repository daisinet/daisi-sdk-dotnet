using Daisi.SDK.Interfaces.Authentication;
using Daisi.SDK.Web.Services;
using Microsoft.AspNetCore.Http;

namespace Daisi.SDK.Web.Providers
{
    public class CookieClientKeyProvider(IHttpContextAccessor httpContextAccessor) : IClientKeyProvider
    {
        public string GetClientKey()
        {
            return httpContextAccessor.HttpContext?.Request.Cookies[AuthService.CLIENT_KEY_STORAGE_KEY] ?? string.Empty;
        }
    }
}
