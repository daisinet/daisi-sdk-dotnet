using Daisi.Protos.V1;
using Daisi.SDK.Clients.V1.Orc;
using Daisi.SDK.Interfaces.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.SDK.Models
{
    public class DefaultClientKeyProvider : IClientKeyProvider
    {
        IServiceProvider serviceProvider => DaisiStaticSettings.Services;
        private string[] accessToIds;

        public DefaultClientKeyProvider() { }
        public DefaultClientKeyProvider( params string[] accessToIds) { 
            
            this.accessToIds = accessToIds;
        }
        public string GetClientKey()
        {
            if (DaisiStaticSettings.ClientKeyExpiresOn < DateTime.UtcNow)
            {
                var scope = serviceProvider.CreateScope();
                var authClientFactory = scope.ServiceProvider.GetService<AuthClientFactory>();
                authClientFactory.CreateStaticClientKey(accessToIds);                
            }

            return DaisiStaticSettings.ClientKey;
        }
    }
}
