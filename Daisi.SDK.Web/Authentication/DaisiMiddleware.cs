using Daisi.SDK.Models;
using Daisi.SDK.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;

namespace Daisi.SDK.Web.Authentication
{
    public class DaisiMiddleware(AuthService authService, SsoTicketService ssoTicketService, ILogger<DaisiMiddleware> logger) : IMiddleware
    {

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var url = context.Request.GetDisplayUrl();
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // SSO Authorize endpoint — the SSO authority (Manager) serves this.
            // Relying-party apps redirect unauthenticated users here.
            if (path == "/sso/authorize")
            {
                await HandleSsoAuthorize(context);
                return;
            }

            // SSO Callback endpoint — the relying-party app (Drive) serves this.
            // Receives an encrypted ticket from the authority and establishes a local session.
            if (path == "/sso/callback")
            {
                await HandleSsoCallback(context);
                return;
            }

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
                await authService.GlobalLogoutAsync();

            }

            await next(context);

        }

        private async Task HandleSsoAuthorize(HttpContext context)
        {
            var returnUrl = context.Request.Query["returnUrl"].ToString();
            var origin = context.Request.Query["origin"].ToString();

            // Validate origin against the allowlist
            var allowedOrigins = DaisiStaticSettings.SsoAllowedOrigins;
            if (allowedOrigins is null || allowedOrigins.Length == 0
                || string.IsNullOrEmpty(origin)
                || !allowedOrigins.Any(o => o.Equals(origin, StringComparison.OrdinalIgnoreCase)))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid or missing SSO origin.");
                return;
            }

            if (string.IsNullOrEmpty(returnUrl))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Missing returnUrl parameter.");
                return;
            }

            // Check if user is authenticated
            var isAuthenticated = false;
            try
            {
                isAuthenticated = await authService.IsAuthenticatedAsync();
            }
            catch { }

            if (!isAuthenticated)
            {
                // Redirect to Manager's own login page, preserving the SSO flow in returnUrl
                var encodedReturnUrl = Uri.EscapeDataString($"/sso/authorize?returnUrl={Uri.EscapeDataString(returnUrl)}&origin={Uri.EscapeDataString(origin)}");
                context.Response.Redirect($"/account/login?returnUrl={encodedReturnUrl}");
                return;
            }

            // User is authenticated — build the ticket from cookies
            var clientKey = context.Request.Cookies[AuthService.CLIENT_KEY_STORAGE_KEY] ?? "";
            var keyExpiration = context.Request.Cookies[AuthService.KEY_EXPIRATION_STORAGE_KEY] ?? "";
            var userName = context.Request.Cookies[AuthService.USER_NAME_STORAGE_KEY] ?? "";
            var userRole = context.Request.Cookies[AuthService.USER_ROLE_KEY] ?? "";
            var accountName = context.Request.Cookies[AuthService.ACCOUNT_NAME_STORAGE_KEY] ?? "";
            var accountId = context.Request.Cookies[AuthService.ACCOUNT_ID_STORAGE_KEY] ?? "";

            var ticket = ssoTicketService.CreateTicket(clientKey, keyExpiration, userName, userRole, accountName, accountId);

            var separator = returnUrl.Contains('?') ? "&" : "?";
            context.Response.Redirect($"{returnUrl}{separator}ticket={Uri.EscapeDataString(ticket)}");
        }

        private async Task HandleSsoCallback(HttpContext context)
        {
            var ticket = context.Request.Query["ticket"].ToString();

            if (string.IsNullOrEmpty(ticket))
            {
                RedirectToSsoAuthority(context);
                return;
            }

            var payload = ssoTicketService.ValidateTicket(ticket);
            if (payload is null)
            {
                logger.LogWarning("SSO callback received invalid or expired ticket");
                RedirectToSsoAuthority(context);
                return;
            }

            // Set all auth cookies from the ticket payload
            await authService.SetClientKeyAsync(payload.ClientKey, DateTime.TryParse(payload.KeyExpiration, out var exp) ? exp : DateTime.UtcNow.AddHours(24));
            await authService.SetUserNameAsync(payload.UserName);
            await authService.SetUserRoleAsync(payload.UserRole);
            await authService.SetAccountNameAsync(payload.AccountName);
            await authService.SetAccountIdAsync(payload.AccountId);

            context.Response.Redirect("/");
        }

        private void RedirectToSsoAuthority(HttpContext context)
        {
            var authorityUrl = DaisiStaticSettings.SsoAuthorityUrl;
            var appUrl = DaisiStaticSettings.SsoAppUrl;

            if (string.IsNullOrEmpty(authorityUrl) || string.IsNullOrEmpty(appUrl))
            {
                context.Response.StatusCode = 500;
                context.Response.WriteAsync("SSO is not configured.");
                return;
            }

            var callbackUrl = $"{appUrl}/sso/callback";
            var authorizeUrl = $"{authorityUrl}/sso/authorize?returnUrl={Uri.EscapeDataString(callbackUrl)}&origin={Uri.EscapeDataString(appUrl)}";
            context.Response.Redirect(authorizeUrl);
        }
    }
}
