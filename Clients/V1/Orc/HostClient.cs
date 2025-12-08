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
    public class HostClientFactory(IClientKeyProvider clientKeyProvider)
    {
        public HostClientFactory() : this(DaisiStaticSettings.DefaultClientKeyProvider) { }

        public HostClient Create(string? orcDomainOrIp = default, int? orcPort = null)
        {
            return new HostClient(orcDomainOrIp ?? DaisiStaticSettings.OrcIpAddressOrDomain, orcPort ?? DaisiStaticSettings.OrcPort, clientKeyProvider);
        }
    }

    public class HostClient : HostsProto.HostsProtoClient
    {

        HostClient(GrpcChannel orcChannel, IClientKeyProvider clientKeyProvider)
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

        internal HostClient(string orcDomainOrIp, int orcPort, IClientKeyProvider clientKeyProvider) 
            : this(GrpcChannel.ForAddress($"{(DaisiStaticSettings.OrcUseSSL ? "https" : "http")}://{orcDomainOrIp}:{orcPort}"), clientKeyProvider)
        {
        }

        public GetHostsResponse GetHosts(CallOptions options) => GetHosts(new(), options);
        public GetHostsResponse GetHosts() => GetHosts(new());

        public AsyncUnaryCall<GetHostsResponse> GetHostsAsync(CallOptions options) => GetHostsAsync(new(), options);
        public AsyncUnaryCall<GetHostsResponse> GetHostsAsync() => GetHostsAsync(new());

      
    }
}
