using Daisi.Protos.V1;
using Daisi.SDK.Clients.V1.Orc;
using Daisi.SDK.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace Daisi.SDK.Web.Services
{
    public class AuthService(IHttpContextAccessor httpContextAccessor, AuthClientFactory authClientFactory)
    {
        public const string CLIENT_KEY_STORAGE_KEY = "clientKey";
        public const string KEY_EXPIRATION_STORAGE_KEY = "keyExpiration";
        public const string USER_NAME_STORAGE_KEY = "userName";
        public const string ACCOUNT_NAME_STORAGE_KEY = "accountName";
        public const string ACCOUNT_ID_STORAGE_KEY = "accountId";
        public const string USER_ROLE_KEY = "userRole";

        public async Task LogoutAsync()
        {
            httpContextAccessor.HttpContext.Response.Cookies.Delete(CLIENT_KEY_STORAGE_KEY);
            httpContextAccessor.HttpContext.Response.Cookies.Delete(KEY_EXPIRATION_STORAGE_KEY);
            httpContextAccessor.HttpContext.Response.Cookies.Delete(USER_NAME_STORAGE_KEY);
            httpContextAccessor.HttpContext.Response.Cookies.Delete(ACCOUNT_NAME_STORAGE_KEY);
            httpContextAccessor.HttpContext.Response.Cookies.Delete(ACCOUNT_ID_STORAGE_KEY);
            httpContextAccessor.HttpContext.Response.Cookies.Delete(USER_ROLE_KEY);
        }
        public async Task<bool> IsAuthenticatedAsync()
        {
            var httpContext = httpContextAccessor.HttpContext;

            var clientKey = httpContext.Request.Cookies[CLIENT_KEY_STORAGE_KEY];
            if (string.IsNullOrEmpty(clientKey)) return false;

            AuthClient client = authClientFactory.Create();
            var validateResponse = client.ValidateClientKey(new ValidateClientKeyRequest
            {
                SecretKey = DaisiStaticSettings.SecretKey,
                ClientKey = clientKey
            });
            
            if (validateResponse is null
                || !validateResponse.IsValid
                || validateResponse.KeyExpiration.ToDateTime() < DateTime.UtcNow) return false;


            var user = new System.Security.Claims.ClaimsPrincipal();            
            var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, validateResponse.HasUserName ? validateResponse.UserName : string.Empty),
                    new Claim(ClaimTypes.Role, validateResponse.HasUserRole ? validateResponse.UserRole.ToString() : string.Empty),
                    new Claim(ClaimTypes.Sid, validateResponse.HasUserId ? validateResponse.UserId : string.Empty),
                    new Claim(ClaimTypes.GroupSid, validateResponse.HasUserAccountId ? validateResponse.UserAccountId : string.Empty),

                };
            var identity = new ClaimsIdentity(claims, "Custom");
            user.AddIdentity(identity);

            httpContext.User = user;

            return true;
        }

        public async Task<string?> GetClientKeyAsync()
        {
            return httpContextAccessor.HttpContext.Request.Cookies[CLIENT_KEY_STORAGE_KEY];
        }

        public async Task SetClientKeyAsync(string clientKey, DateTime keyExpiration)
        {
            httpContextAccessor.HttpContext.Response.Cookies.Append(CLIENT_KEY_STORAGE_KEY, clientKey);
            httpContextAccessor.HttpContext.Response.Cookies.Append(KEY_EXPIRATION_STORAGE_KEY, keyExpiration.ToString());
        }

        public async Task SetUserNameAsync(string username)
        {
            httpContextAccessor.HttpContext.Response.Cookies.Append(USER_NAME_STORAGE_KEY, username);
        }
      
        public async Task<string?> GetUserNameAsync()
        {
            return httpContextAccessor.HttpContext.Request.Cookies[USER_NAME_STORAGE_KEY];
        }
        public async Task SetUserRoleAsync(string userRole)
        {
            httpContextAccessor.HttpContext.Response.Cookies.Append(USER_ROLE_KEY, userRole);
        }
        public async Task<UserRoles?> GetUserRoleAsync()
        {
            var ur = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            Console.WriteLine($"USER ROLE CLAIM: {ur}");
            if (ur == null) return null;
            if (Enum.TryParse<UserRoles>(ur, out var role))
            {
                Console.WriteLine($"USER ROLE PARSED: {role} ({(int)role})");
                return role;
            }

            // Fallback: try parsing as integer (e.g. "3" -> UserRolesAdmin)
            if (int.TryParse(ur, out var roleInt) && Enum.IsDefined(typeof(UserRoles), roleInt))
            {
                role = (UserRoles)roleInt;
                Console.WriteLine($"USER ROLE PARSED FROM INT: {role} ({roleInt})");
                return role;
            }

            Console.WriteLine($"USER ROLE PARSE FAILED: {ur}");
            return null;
        }
        public async Task SetAccountNameAsync(string accountName)
        {
            httpContextAccessor.HttpContext.Response.Cookies.Append(ACCOUNT_NAME_STORAGE_KEY, accountName);
        }
        public async Task<string?> GetAccountNameAsync()
        {
            return httpContextAccessor.HttpContext.Request.Cookies[ACCOUNT_NAME_STORAGE_KEY];
        }
        public async Task SetAccountIdAsync(string accountId)
        {
            httpContextAccessor.HttpContext.Response.Cookies.Append(ACCOUNT_ID_STORAGE_KEY, accountId);
        }
        public async Task<string?> GetAccountIdAsync()
        {
            httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(ACCOUNT_ID_STORAGE_KEY, out var accountId);
            return accountId;
        }
    }
}
