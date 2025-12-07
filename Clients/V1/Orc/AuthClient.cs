using Daisi.Protos.V1;
using Daisi.SDK.Interfaces;
using Daisi.SDK.Models;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.SDK.Clients.V1.Orc
{
    public class AuthClientFactory(IClientKeyProvider clientKeyProvider)
    {
        public AuthClientFactory() : this(DaisiStaticSettings.DefaultClientKeyProvider) { }

        public AuthClient Create(string? orcDomainOrIp = default, int? orcPort = null)
        {
            return new AuthClient(orcDomainOrIp ?? DaisiStaticSettings.OrcIpAddressOrDomain, orcPort ?? DaisiStaticSettings.OrcPort, clientKeyProvider); 
        }
        
    }
    public class AuthClient : AuthProto.AuthProtoClient
    {

        private AuthClient(GrpcChannel orcChannel, IClientKeyProvider clientKeyProvider)
           : base(orcChannel.Intercept((metadata) =>
           {
               if (clientKeyProvider is not null)
                   metadata.Add(DaisiStaticSettings.ClientKeyHeader, clientKeyProvider.GetClientKey() ?? "NOKEY");
               else
                   metadata.Add(DaisiStaticSettings.ClientKeyHeader, DaisiStaticSettings.ClientKey ?? "NOKEY");
               
               return metadata;
           }))
        { 

        }
        internal AuthClient(string orcDomainOrIp, int orcPort, IClientKeyProvider clientKeyProvider)
            : this(GrpcChannel.ForAddress($"{(DaisiStaticSettings.OrcUseSSL ? "https" : "http")}://{orcDomainOrIp}:{orcPort}"), clientKeyProvider)
        {

        }

      
    }
}
