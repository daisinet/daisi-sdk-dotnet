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
    public class DappClientFactory(IClientKeyProvider clientKeyProvider)
    {
        public DappClientFactory() : this(DaisiStaticSettings.DefaultClientKeyProvider) { }

        public DappClient Create(string? orcDomainOrIp = default, int? orcPort = null)
        {
            return new DappClient(orcDomainOrIp ?? DaisiStaticSettings.OrcIpAddressOrDomain, orcPort ?? DaisiStaticSettings.OrcPort, clientKeyProvider);
        }
    }
    public class DappClient : DappsProto.DappsProtoClient
    {
        DappClient(GrpcChannel orcChannel, IClientKeyProvider clientKeyProvider)
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

        internal DappClient(string orcDomainOrIp, int orcPort, IClientKeyProvider clientKeyProvider)
            : this(GrpcChannel.ForAddress($"{(DaisiStaticSettings.OrcUseSSL ? "https" : "http")}://{orcDomainOrIp}:{orcPort}"), clientKeyProvider)
        {
        }
    }
}
