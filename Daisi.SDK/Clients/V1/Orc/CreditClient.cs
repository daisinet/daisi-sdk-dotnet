using Daisi.Protos.V1;
using Daisi.SDK.Interfaces.Authentication;
using Daisi.SDK.Models;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;

namespace Daisi.SDK.Clients.V1.Orc
{
    public class CreditClientFactory(IClientKeyProvider clientKeyProvider)
    {
        public CreditClientFactory() : this(DaisiStaticSettings.DefaultClientKeyProvider) { }

        public CreditClient Create(string? orcDomainOrIp = default, int? orcPort = null)
        {
            return new CreditClient(orcDomainOrIp ?? DaisiStaticSettings.OrcIpAddressOrDomain, orcPort ?? DaisiStaticSettings.OrcPort, clientKeyProvider);
        }
    }
    public class CreditClient : CreditsProto.CreditsProtoClient
    {

        CreditClient(GrpcChannel orcChannel, IClientKeyProvider clientKeyProvider)
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

        internal CreditClient(string orcDomainOrIp, int orcPort, IClientKeyProvider clientKeyProvider)
            : this(GrpcChannel.ForAddress($"{(DaisiStaticSettings.OrcUseSSL ? "https" : "http")}://{orcDomainOrIp}:{orcPort}"), clientKeyProvider)
        {
        }
    }
}
