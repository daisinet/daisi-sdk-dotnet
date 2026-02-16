using Daisi.SDK.Clients.V1.Host;
using Daisi.SDK.Clients.V1.Orc;
using Daisi.SDK.Clients.V1.SessionManagers;
using Daisi.SDK.Interfaces.Authentication;
using Daisi.SDK.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Daisi.SDK.Extensions
{

    public static class DaisiServicesExtensions
    {
        /// <summary>
        /// Adds the Daisi clients and DefaultClientKeyProvider to your application services.
        /// Supply a secret key only when the application is not distributed, as client applications
        /// can see the key.
        /// </summary>
        /// <param name="services">The app builder's implementation of IServiceCollection. ex: builder.Services</param>
        /// <param name="secretKey">Supply a secret key only when the application is not distributed, as client applications can see the key.</param>
        /// <returns>The same implementation of IServiceCollection with the DAISI services added.</returns>
        public static IServiceCollection AddDaisi(this IServiceCollection services, string? secretKey = default)
        {
            DaisiStaticSettings.SecretKey = secretKey;

            services.AddDaisiClients()
                    .AddDaisiDefaultClientKeyProvider();

            DaisiStaticSettings.AutoswapOrc();

            return services;
        }
        public static IServiceCollection AddDaisiClients(this IServiceCollection services)
        {            
            services.AddDaisiHostClients();
            services.AddDaisiOrcClients();
            return services;
        }
        public static IServiceCollection AddDaisiHostClients(this IServiceCollection services)
        {
            services.AddTransient<InferenceClientFactory>();
            services.AddTransient<InferenceSessionManager>();

            services.AddTransient<PeerClientFactory>();
            services.AddTransient<PeerSessionManager>();

            services.AddTransient<SettingsClientFactory>();
            services.AddTransient<SettingsSessionManager>();
            return services;
        }

        public static IServiceCollection AddDaisiOrcClients(this IServiceCollection services)
        {
            services.AddSingleton<AccountClientFactory>();
            services.AddSingleton<CommandClientFactory>();
            services.AddSingleton<CreditClientFactory>();
            services.AddSingleton<HostClientFactory>();
            services.AddSingleton<ModelClientFactory>();
            services.AddSingleton<SessionClientFactory>();
            services.AddSingleton<AuthClientFactory>();
            services.AddSingleton<ReleaseClientFactory>();
            services.AddSingleton<SkillClientFactory>();
            services.AddSingleton<DriveClientFactory>();
            services.AddSingleton<MarketplaceClientFactory>();
            services.AddSingleton<SecureToolClientFactory>();
            return services;
        }


        public static IServiceCollection AddDaisiDefaultClientKeyProvider(this IServiceCollection services)
        {            
            services.AddSingleton<IClientKeyProvider, DefaultClientKeyProvider>();
            return services;
        }

       
        public static IServiceProvider UseDaisi(this IServiceProvider serviceProvider)
        {
            if (!string.IsNullOrWhiteSpace(DaisiStaticSettings.SecretKey))
            {
                var authClientFactory = serviceProvider.GetService<AuthClientFactory>();
                var authClient = authClientFactory.Create();
                DaisiStaticSettings.ClientKey = string.Empty;
                var response = authClient.CreateClientKey(new Protos.V1.CreateClientKeyRequest() { SecretKey = DaisiStaticSettings.SecretKey });
                DaisiStaticSettings.ClientKey = response.ClientKey;
            }

            DaisiStaticSettings.DefaultClientKeyProvider = serviceProvider.GetService<IClientKeyProvider>();

            return serviceProvider;
        }

    }
}
