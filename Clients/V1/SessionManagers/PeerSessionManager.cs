using Daisi.SDK.Clients.V1.Host;
using Daisi.SDK.Clients.V1.Orc;
using Daisi.SDK.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.SDK.Clients.V1.SessionManagers
{
    public class PeerSessionManager : SessionManagerBase<PeerClient>
    {
        public PeerSessionManager(SessionClientFactory sessionClientFactory, IClientKeyProvider clientKeyProvider) : base(sessionClientFactory, clientKeyProvider, NullLogger.Instance)
        {
        }
    }
}
