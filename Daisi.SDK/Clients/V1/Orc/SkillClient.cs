using Daisi.Protos.V1;
using Daisi.SDK.Interfaces.Authentication;
using Daisi.SDK.Models;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;

namespace Daisi.SDK.Clients.V1.Orc
{
    public class SkillClientFactory(IClientKeyProvider clientKeyProvider)
    {
        public SkillClientFactory() : this(DaisiStaticSettings.DefaultClientKeyProvider) { }

        public SkillClient Create(string? orcDomainOrIp = default, int? orcPort = null)
        {
            return new SkillClient(orcDomainOrIp ?? DaisiStaticSettings.OrcIpAddressOrDomain, orcPort ?? DaisiStaticSettings.OrcPort, clientKeyProvider);
        }
    }

    public class SkillClient : SkillsProto.SkillsProtoClient
    {
        SkillClient(GrpcChannel orcChannel, IClientKeyProvider clientKeyProvider)
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

        internal SkillClient(string orcDomainOrIp, int orcPort, IClientKeyProvider clientKeyProvider)
            : this(GrpcChannel.ForAddress($"{(DaisiStaticSettings.OrcUseSSL ? "https" : "http")}://{orcDomainOrIp}:{orcPort}"), clientKeyProvider)
        {
        }
    }
}
