using Daisi.SDK.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Daisi.SDK.Web.Authentication
{
    public class DaisiMiddleware(AuthService authService, ILogger<DaisiMiddleware> logger) : IMiddleware
    {

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var url = context.Request.GetDisplayUrl();

            if (url.ToLower().Contains("setauthcookies"))
            {
                logger.LogInformation("SetAuthCookies in DaisiMiddleware (IMiddleware)");
                var accountName = context.Request.Query["accountName"];
                var accountId = context.Request.Query["accountId"];
                var userName = context.Request.Query["userName"];
                var userRole = context.Request.Query["userRole"];
                var clientKey = context.Request.Query["clientKey"];
                var keyExpiration = context.Request.Query["keyExpiration"];

                await authService.SetAccountIdAsync(accountId);
                await authService.SetAccountNameAsync(accountName);
                await authService.SetUserNameAsync(userName);
                await authService.SetUserRoleAsync(userRole);
                await authService.SetClientKeyAsync(clientKey, DateTime.Parse(keyExpiration));


                logger.LogInformation("SetAuthCookies in DaisiMiddleware (IMiddleware)");

            }

            var isAuthenticated = false;
            try
            {
                isAuthenticated = await authService.IsAuthenticatedAsync();
            }
            catch (Exception ex)
            {
                logger.LogInformation("UNAUTH Logout in DaisiMiddleware (IMiddleware)");
                await authService.LogoutAsync();
            }

            if (url.ToLower().Contains("logout") && isAuthenticated)
            {
                logger.LogInformation("MANUAL Logout in DaisiMiddleware (IMiddleware)");
                await authService.LogoutAsync();

            }

            await next(context);

        }
    }
}
