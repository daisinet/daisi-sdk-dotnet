using Daisi.Protos.V1;
using Daisi.SDK.Clients.V1.Orc;
using Daisi.SDK.Clients.V1.SessionManagers;
using Daisi.SDK.Interfaces;
using Daisi.SDK.Models;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.SDK.Clients.V1.Host
{
    public class PeerClientFactory(PeerSessionManager sessionManager)
        : FullyOrchestratedClientFactory<PeerClient>(sessionManager)
    {
        public PeerClientFactory() 
            : this( new PeerSessionManager(new SessionClientFactory(), DaisiStaticSettings.DefaultClientKeyProvider) )
        {

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
