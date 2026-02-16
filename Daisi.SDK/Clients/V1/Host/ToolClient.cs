using Daisi.Protos.V1;
using Grpc.Net.Client;

namespace Daisi.SDK.Clients.V1.Host
{
    /// <summary>
    /// Client for direct connect tool execution on tools-only hosts.
    /// Connects to a tools-only host's ToolsRPC service at the specified address.
    /// </summary>
    public class ToolClient : ToolsProto.ToolsProtoClient
    {
        public ToolClient(string hostIpAddress, int port = 4242)
            : base(GrpcChannel.ForAddress($"http://{hostIpAddress}:{port}"))
        {
        }

        /// <summary>
        /// Executes a tool on the remote tools-only host.
        /// </summary>
        public async Task<ExecuteToolResponse> ExecuteToolAsync(ExecuteToolRequest request, CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(request, cancellationToken: cancellationToken);
        }
    }
}
