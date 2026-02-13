using Daisi.Protos.V1;
using Daisi.SDK.Clients.V1.Host;
using Daisi.SDK.Clients.V1.Orc;
using Daisi.SDK.Clients.V1.SessionManagers;
using Daisi.SDK.Interfaces.Authentication;
using Daisi.SDK.Web.Authentication;
using Daisi.SDK.Web.Authorization;
using Daisi.SDK.Web.Providers;
using Daisi.SDK.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Daisi.SDK.Web.Extensions
{
    public static class WebStartupExtensions
    {
        public static IApplicationBuilder UseDaisiMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<DaisiMiddleware>();
            return app;
        }
        public static IServiceCollection AddDaisiForWeb(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddScoped<AuthService>();
            services.AddSingleton<SsoTicketService>();

            // Orc Clients
            services.AddScoped<AccountClientFactory>();
            services.AddScoped<AuthClientFactory>();
            services.AddScoped<DappClientFactory>();
            services.AddScoped<HostClientFactory>();
            services.AddScoped<ModelClientFactory>();
            services.AddScoped<NetworkClientFactory>();
            services.AddScoped<SessionClientFactory>();
            services.AddScoped<OrcClientFactory>();
            services.AddScoped<ReleaseClientFactory>();
            services.AddScoped<SkillClientFactory>();
            services.AddScoped<DriveClientFactory>();
            services.AddScoped<CreditClientFactory>();
            services.AddScoped<MarketplaceClientFactory>();

            // Host Clients
            services.AddTransient<InferenceClientFactory>();
            services.AddTransient<InferenceSessionManager>();

            services.AddTransient<PeerClientFactory>();
            services.AddTransient<PeerSessionManager>();

            services.AddTransient<SettingsClientFactory>();
            services.AddTransient<SettingsSessionManager>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {
                        options.LoginPath = "/account/login";
                        options.AccessDeniedPath = "/account/denied";
                    });

            services.AddAuthorizationCore(options =>
            {
                options.InvokeHandlersAfterFailure = false;

                options.AddPolicy(ManagerAttribute.DaisiManagerPolicyName, policy =>
                    policy.RequireClaim(ClaimTypes.Role, UserRoles.Manager.ToString(), UserRoles.Owner.ToString()));

                options.AddPolicy(OwnerAttribute.DaisiOwnerPolicyName, policy =>
                    policy.RequireClaim(ClaimTypes.Role, UserRoles.Owner.ToString()));
            });

            return services;
        }

        public static IServiceCollection AddDaisiMiddleware(this IServiceCollection services)
        {
            services.AddTransient<DaisiMiddleware>();
            return services;
        }

        public static IServiceCollection AddDaisiCookieKeyProvider(this IServiceCollection services)
        {

            services.AddScoped<IClientKeyProvider>((services) =>
            {
                var httpContextAccessor = services.GetService<IHttpContextAccessor>();
                var provider = new CookieClientKeyProvider(httpContextAccessor);
                return provider;
            });
            return services;
        }
    }
}
