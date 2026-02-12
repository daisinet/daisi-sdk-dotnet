using Daisi.Protos.V1;
using Grpc.Net.Client;

namespace Daisi.SDK.Clients.V1
{
    /// <summary>
    /// Simple gRPC client for ORC → File Manager communication.
    /// Used to notify the File Manager of host lifecycle events.
    /// </summary>
    public class DriveNotificationClient : DriveNotificationProto.DriveNotificationProtoClient
    {
        public DriveNotificationClient(string fileManagerUrl)
            : base(GrpcChannel.ForAddress(fileManagerUrl))
        {
        }

        public DriveNotificationClient(GrpcChannel channel) : base(channel)
        {
        }
    }
}
