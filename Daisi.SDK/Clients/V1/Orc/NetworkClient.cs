using Daisi.Protos.V1;
using Daisi.SDK.Interfaces.Authentication;
using Daisi.SDK.Models;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.SDK.Clients.V1.Orc
{
    public class NetworkClientFactory(IClientKeyProvider clientKeyProvider)
    {
        public NetworkClientFactory() : this(DaisiStaticSettings.DefaultClientKeyProvider) { }
        public NetworkClient Create(string? orcDomainOrIp = default, int? orcPort = null)
        {
            return new NetworkClient(orcDomainOrIp ?? DaisiStaticSettings.OrcIpAddressOrDomain, orcPort ?? DaisiStaticSettings.OrcPort, clientKeyProvider);
        }
    }
    public partial class NetworkClient : NetworksProto.NetworksProtoClient
    {
        private NetworkClient(GrpcChannel orcChannel, IClientKeyProvider clientKeyProvider)
        : base(orcChannel.Intercept((metadata) =>
        {
            if (clientKeyProvider is not null)
                metadata.Add(DaisiStaticSettings.ClientKeyHeader, clientKeyProvider.GetClientKey());
            else
                metadata.Add(DaisiStaticSettings.ClientKeyHeader, DaisiStaticSettings.ClientKey);

            return metadata;
        }))
        {

        }

        internal NetworkClient(string orcDomainOrIp, int orcPort, IClientKeyProvider clientKeyProvider)
            : this(GrpcChannel.ForAddress($"{(DaisiStaticSettings.OrcUseSSL ? "https" : "http")}://{orcDomainOrIp}:{orcPort}"), clientKeyProvider)
        {

        }


    }
}
