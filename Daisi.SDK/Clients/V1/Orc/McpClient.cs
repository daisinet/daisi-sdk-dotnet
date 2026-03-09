using Daisi.Protos.V1;
using Daisi.SDK.Interfaces.Authentication;
using Daisi.SDK.Models;
using Daisi.SDK.Providers;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;

namespace Daisi.SDK.Clients.V1.Orc
{
    public class McpClientFactory(IClientKeyProvider clientKeyProvider)
    {
        public McpClientFactory() : this(DaisiStaticSettings.DefaultClientKeyProvider) { }

        public McpClient Create(string? orcDomainOrIp = default, int? orcPort = null)
        {
            return new McpClient(
                orcDomainOrIp ?? DaisiStaticSettings.OrcIpAddressOrDomain,
                orcPort ?? DaisiStaticSettings.OrcPort,
                clientKeyProvider);
        }
    }

    public class McpClient : McpProto.McpProtoClient
    {
        McpClient(GrpcChannel channel, IClientKeyProvider clientKeyProvider)
            : base(channel.Intercept((metadata) =>
            {
                if (clientKeyProvider is not null)
                    metadata.Add(DaisiStaticSettings.ClientKeyHeader, clientKeyProvider.GetClientKey());
                else
                    metadata.Add(DaisiStaticSettings.ClientKeyHeader, DaisiStaticSettings.ClientKey);

                if (clientKeyProvider is IDriveIdentityProvider identity)
                {
                    metadata.Add("x-daisi-account-id", identity.GetAccountId());
                    metadata.Add("x-daisi-user-id", identity.GetUserId());
                    metadata.Add("x-daisi-user-name", identity.GetUserName());
                    metadata.Add("x-daisi-user-role", identity.GetUserRole().ToString());
                }

                return metadata;
            }))
        {
        }

        internal McpClient(string domainOrIp, int port, IClientKeyProvider clientKeyProvider)
            : this(GrpcChannel.ForAddress($"{(DaisiStaticSettings.OrcUseSSL ? "https" : "http")}://{domainOrIp}:{port}"), clientKeyProvider)
        {
        }

        /// <summary>
        /// Registers a new MCP server for the current account.
        /// </summary>
        public async Task<RegisterMcpServerResponse> RegisterServerAsync(
            string name, string serverUrl, McpAuthType authType, string authSecret,
            int syncIntervalMinutes = 60, string? targetRepositoryId = null,
            CancellationToken cancellationToken = default)
        {
            var request = new RegisterMcpServerRequest
            {
                Name = name,
                ServerUrl = serverUrl,
                AuthType = authType,
                AuthSecret = authSecret,
                SyncIntervalMinutes = syncIntervalMinutes
            };
            if (!string.IsNullOrEmpty(targetRepositoryId))
                request.TargetRepositoryId = targetRepositoryId;
            return await RegisterServerAsync(request, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Lists all MCP servers for the current account.
        /// </summary>
        public async Task<ListMcpServersResponse> ListServersAsync(CancellationToken cancellationToken = default)
        {
            return await ListServersAsync(new ListMcpServersRequest(), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Gets a specific MCP server by ID.
        /// </summary>
        public async Task<GetMcpServerResponse> GetServerAsync(string serverId, CancellationToken cancellationToken = default)
        {
            return await GetServerAsync(new GetMcpServerRequest { ServerId = serverId }, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Removes an MCP server and its associated data.
        /// </summary>
        public async Task<RemoveMcpServerResponse> RemoveServerAsync(string serverId, CancellationToken cancellationToken = default)
        {
            return await RemoveServerAsync(new RemoveMcpServerRequest { ServerId = serverId }, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Triggers an immediate sync for an MCP server.
        /// </summary>
        public async Task<TriggerMcpSyncResponse> TriggerSyncAsync(string serverId, CancellationToken cancellationToken = default)
        {
            return await TriggerSyncAsync(new TriggerMcpSyncRequest { ServerId = serverId }, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Gets the current status of an MCP server.
        /// </summary>
        public async Task<GetMcpServerStatusResponse> GetServerStatusAsync(string serverId, CancellationToken cancellationToken = default)
        {
            return await GetServerStatusAsync(new GetMcpServerStatusRequest { ServerId = serverId }, cancellationToken: cancellationToken);
        }
    }
}
