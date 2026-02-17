using Daisi.Protos.V1;
using Daisi.SDK.Interfaces.Authentication;
using Daisi.SDK.Models;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;

namespace Daisi.SDK.Clients.V1.Orc
{
    public class BlogClientFactory(IClientKeyProvider clientKeyProvider)
    {
        public BlogClientFactory() : this(DaisiStaticSettings.DefaultClientKeyProvider)
        {

        }
        public BlogClient Create(string? orcDomainOrIp = default, int? orcPort = null)
        {
            return new BlogClient(orcDomainOrIp ?? DaisiStaticSettings.OrcIpAddressOrDomain, orcPort ?? DaisiStaticSettings.OrcPort, clientKeyProvider);
        }
    }
    public partial class BlogClient : BlogsProto.BlogsProtoClient
    {
        private BlogClient(GrpcChannel orcChannel, IClientKeyProvider clientKeyProvider)
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
        internal BlogClient(string orcDomainOrIp, int orcPort, IClientKeyProvider clientKeyProvider)
            : this(GrpcChannel.ForAddress($"{(DaisiStaticSettings.OrcUseSSL ? "https" : "http")}://{orcDomainOrIp}:{orcPort}"), clientKeyProvider)
        {

        }
    }
}
