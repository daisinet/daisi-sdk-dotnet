using Daisi.Protos.V1;
using Daisi.SDK.Interfaces.Authentication;
using Daisi.SDK.Models;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;

namespace Daisi.SDK.Clients.V1.Orc
{
    public class MarketplaceClientFactory(IClientKeyProvider clientKeyProvider)
    {
        public MarketplaceClientFactory() : this(DaisiStaticSettings.DefaultClientKeyProvider) { }

        public MarketplaceClient Create(string? orcDomainOrIp = default, int? orcPort = null)
        {
            return new MarketplaceClient(orcDomainOrIp ?? DaisiStaticSettings.OrcIpAddressOrDomain, orcPort ?? DaisiStaticSettings.OrcPort, clientKeyProvider);
        }
    }

    public class MarketplaceClient : MarketplaceProto.MarketplaceProtoClient
    {
        MarketplaceClient(GrpcChannel orcChannel, IClientKeyProvider clientKeyProvider)
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

        internal MarketplaceClient(string orcDomainOrIp, int orcPort, IClientKeyProvider clientKeyProvider)
            : this(GrpcChannel.ForAddress($"{(DaisiStaticSettings.OrcUseSSL ? "https" : "http")}://{orcDomainOrIp}:{orcPort}"), clientKeyProvider)
        {
        }
    }
}
