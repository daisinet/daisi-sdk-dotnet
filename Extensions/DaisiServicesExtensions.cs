using Daisi.SDK.Clients.V1.Host;
using Daisi.SDK.Clients.V1.Orc;
using Daisi.SDK.Clients.V1.SessionManagers;
using Daisi.SDK.Interfaces;
using Daisi.SDK.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Daisi.SDK.Extensions
{

    public static class DaisiServicesExtensions
    {
        public static IServiceCollection AddDaisi(this IServiceCollection services, string secretKey)
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
            services.AddSingleton<CommandClientFactory>();
            services.AddSingleton<HostClientFactory>();
            services.AddSingleton<ModelClientFactory>();
            services.AddSingleton<SessionClientFactory>();
            services.AddSingleton<AuthClientFactory>();
            return services;
        }


        public static IServiceCollection AddDaisiDefaultClientKeyProvider(this IServiceCollection services)
        {            
            services.AddSingleton<IClientKeyProvider, DefaultClientKeyProvider>();
            return services;
        }

       
        public static IServiceProvider UseDaisi(this IServiceProvider serviceProvider)
        {
            var authClientFactory = serviceProvider.GetService<AuthClientFactory>();
            var authClient = authClientFactory.Create();
            DaisiStaticSettings.ClientKey = string.Empty;
            var response = authClient.CreateClientKey(new Protos.V1.CreateClientKeyRequest() { SecretKey = DaisiStaticSettings.SecretKey });
            DaisiStaticSettings.ClientKey = response.ClientKey;
            return serviceProvider;
        }

    }
}
