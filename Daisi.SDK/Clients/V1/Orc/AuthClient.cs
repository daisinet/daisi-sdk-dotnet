using Daisi.Protos.V1;
using Daisi.SDK.Interfaces.Authentication;
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
        
        /// <summary>
        /// Calls the Orc using the DaisiStaticSettings.SecretKey to create and set the DaisiStaticSettings.ClientKey.
        /// </summary>
        public void CreateStaticClientKey(params string[] accessToIds)
        {
            var authClient = Create();
            DaisiStaticSettings.ClientKey = string.Empty;
            var clientKeyRequest = new Protos.V1.CreateClientKeyRequest() { SecretKey = DaisiStaticSettings.SecretKey };
            if (accessToIds is not null && accessToIds.Length > 0)
            {
                clientKeyRequest.AccessToIds.AddRange(accessToIds);
            }
            var response = authClient.CreateClientKey(clientKeyRequest);
            DaisiStaticSettings.ClientKey = response.ClientKey;
            DaisiStaticSettings.ClientKeyExpiresOn = response.KeyExpiration.ToDateTime();
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

        public GetAuthenticatedUserResponse GetAuthenticatedUser() => GetAuthenticatedUser(new());
        public async Task<GetAuthenticatedUserResponse> GetAuthenticatedUserAsync() => await GetAuthenticatedUserAsync(new());
    }
}
