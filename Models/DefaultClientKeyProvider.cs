using Daisi.Protos.V1;
using Daisi.SDK.Clients.V1.Orc;
using Daisi.SDK.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.SDK.Models
{
    public class DefaultClientKeyProvider(AuthClient authClient) : IClientKeyProvider
    {
        public string GetClientKey()
        {
            var result = authClient.CreateClientKey(new CreateClientKeyRequest
            {
                SecretKey = DaisiStaticSettings.SecretKey
            });

            DaisiStaticSettings.ClientKey = result.ClientKey;
            return result.ClientKey;
        }
    }
}
