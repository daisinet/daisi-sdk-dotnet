using Daisi.SDK.Models;
using Grpc.Core;
using Grpc.Net.Client;

namespace Daisi.SDK.Helpers
{
    public class ChannelHelper
    {
        private static GrpcChannel CreateOrcAuthenticatedChannel()
        {
            var credentials = CallCredentials.FromInterceptor(async (context, metadata) =>
            {
                metadata.Add(DaisiStaticSettings.ClientKeyHeader, $"{DaisiStaticSettings.ClientKey}");
            });

            var channel = GrpcChannel.ForAddress(DaisiStaticSettings.OrcUrl, new GrpcChannelOptions
            {
                Credentials = DaisiStaticSettings.OrcUseSSL 
                                ? ChannelCredentials.Create(ChannelCredentials.SecureSsl, credentials)
                                : ChannelCredentials.Create(ChannelCredentials.Insecure, credentials)
            });
            return channel;
        }
    }
}
