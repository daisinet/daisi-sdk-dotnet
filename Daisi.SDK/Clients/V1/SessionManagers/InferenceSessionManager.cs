using Daisi.SDK.Clients.V1.Host;
using Daisi.SDK.Clients.V1.Orc;
using Daisi.SDK.Interfaces.Authentication;
using Daisi.SDK.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.SDK.Clients.V1.SessionManagers
{
    public class InferenceSessionManager : SessionManagerBase<InferenceClient>
    {
        public InferenceSessionManager():base(new SessionClientFactory(), 
                                                DaisiStaticSettings.DefaultClientKeyProvider, 
                                                NullLogger.Instance)
        {

        }
        public InferenceSessionManager(SessionClientFactory sessionClientFactory, 
            IClientKeyProvider clientKeyProvider, 
            ILogger<InferenceSessionManager> logger) : base(sessionClientFactory, clientKeyProvider, logger)
        {

        }

        public InferenceSessionManager(SessionClientFactory sessionClientFactory,
           IClientKeyProvider clientKeyProvider,
           ILogger logger) : base(sessionClientFactory, clientKeyProvider, logger)
        {

        }
    }
}
