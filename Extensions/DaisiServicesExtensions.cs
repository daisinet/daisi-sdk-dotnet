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
        public static void AddDaisi(this IServiceCollection services, string secretKey)
        {
            DaisiStaticSettings.SecretKey = secretKey;
            services.AddDaisiClients();
            services.AddDaisiDefaultClientKeyProvider();
            DaisiStaticSettings.AutoswapOrc();
        }
        public static void AddDaisiClients(this IServiceCollection services)
        {            
            services.AddDaisiHostClients();
            services.AddDaisiOrcClients();
        }
        public static void AddDaisiHostClients(this IServiceCollection services)
        {
            services.AddTransient<InferenceClientFactory>();
            services.AddTransient<InferenceSessionManager>();

            services.AddTransient<PeerClientFactory>();
            services.AddTransient<PeerSessionManager>();

            services.AddTransient<SettingsClientFactory>();
            services.AddTransient<SettingsSessionManager>();
        }

        public static void AddDaisiOrcClients(this IServiceCollection services)
        {
            services.AddSingleton<CommandClientFactory>();
            services.AddSingleton<HostClientFactory>();
            services.AddSingleton<ModelClientFactory>();
            services.AddSingleton<SessionClientFactory>();
            services.AddSingleton<AuthClientFactory>();
        }


        public static void AddDaisiDefaultClientKeyProvider(this IServiceCollection services)
        {            
            services.AddSingleton<IClientKeyProvider, DefaultClientKeyProvider>();
        }

      
    }
}
