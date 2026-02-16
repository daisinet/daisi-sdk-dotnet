using Daisi.Protos.V1;
using Daisi.SDK.Interfaces.Authentication;
using Daisi.SDK.Models;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;

namespace Daisi.SDK.Clients.V1.Orc
{
    /// <summary>
    /// Factory for creating <see cref="SecureToolClient"/> instances that communicate with the ORC's SecureTool gRPC service.
    /// </summary>
    public class SecureToolClientFactory(IClientKeyProvider clientKeyProvider)
    {
        /// <summary>
        /// Creates a new factory using the default client key provider.
        /// </summary>
        public SecureToolClientFactory() : this(DaisiStaticSettings.DefaultClientKeyProvider) { }

        /// <summary>
        /// Creates a new <see cref="SecureToolClient"/> connected to the configured ORC.
        /// </summary>
        public SecureToolClient Create(string? orcDomainOrIp = default, int? orcPort = null)
        {
            return new SecureToolClient(orcDomainOrIp ?? DaisiStaticSettings.OrcIpAddressOrDomain, orcPort ?? DaisiStaticSettings.OrcPort, clientKeyProvider);
        }
    }

    /// <summary>
    /// gRPC client for the SecureTool service on the ORC. Queries installed secure tools
    /// with their InstallId and EndpointUrl for direct provider communication.
    /// </summary>
    public class SecureToolClient : SecureToolProto.SecureToolProtoClient
    {
        SecureToolClient(GrpcChannel orcChannel, IClientKeyProvider clientKeyProvider)
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

        internal SecureToolClient(string orcDomainOrIp, int orcPort, IClientKeyProvider clientKeyProvider)
            : this(GrpcChannel.ForAddress($"{(DaisiStaticSettings.OrcUseSSL ? "https" : "http")}://{orcDomainOrIp}:{orcPort}"), clientKeyProvider)
        {
        }
    }
}
