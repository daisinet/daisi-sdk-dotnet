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
    public class ModelClientFactory(IClientKeyProvider clientKeyProvider)
    {
        public ModelClientFactory() : this(DaisiStaticSettings.DefaultClientKeyProvider)
        {

        }
        public ModelClient Create(string? orcDomainOrIp = default, int? orcPort = null)
        {
            return new ModelClient(orcDomainOrIp ?? DaisiStaticSettings.OrcIpAddressOrDomain, orcPort ?? DaisiStaticSettings.OrcPort, clientKeyProvider);
        }
    }
    public partial class ModelClient : ModelsProto.ModelsProtoClient
    {
        private ModelClient(GrpcChannel orcChannel, IClientKeyProvider clientKeyProvider)
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
        internal ModelClient(string orcDomainOrIp, int orcPort, IClientKeyProvider clientKeyProvider)
            : this(GrpcChannel.ForAddress($"{(DaisiStaticSettings.OrcUseSSL ? "https" : "http")}://{orcDomainOrIp}:{orcPort}"), clientKeyProvider)
        {

        }

        public GetRequiredModelsResponse GetRequiredModels(CallOptions options) => GetRequiredModels(new(), options);
        public GetRequiredModelsResponse GetRequiredModels() => GetRequiredModels(new());

    }
}
