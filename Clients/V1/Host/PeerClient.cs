using Daisi.Protos.V1;
using Daisi.SDK.Clients.V1.Orc;
using Daisi.SDK.Interfaces;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.SDK.Clients.V1.Host
{
    public class PeerClientFactory(IClientKeyProvider clientKeyProvider, SessionClientFactory sessionClientFactory)
    {
        public PeerClient Create(string hostIpAddress, int hostPort = 4242, string? orcDomainOrIp = default, int? orcPort = null)
        {
            var client = new PeerClient(clientKeyProvider, sessionClientFactory.Create(orcDomainOrIp, orcPort), hostIpAddress, hostPort);
            return client;
        }
    }
    public partial class PeerClient : PeersProto.PeersProtoClient
    {
        public IClientKeyProvider ClientKeyProvider { get; }
        public SessionClient SessionClient { get; }

        internal PeerClient(IClientKeyProvider clientKeyProvider, SessionClient sessionClient, string hostIpAddress, int port = 4242) 
            : base(GrpcChannel.ForAddress($"http://{hostIpAddress}:{port}")) {
            this.ClientKeyProvider = clientKeyProvider;
            this.SessionClient = sessionClient;

            Initialize();
        }

        protected void Initialize()
        {

        }

    }
}
