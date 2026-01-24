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
    public class SettingsSessionManager : SessionManagerBase<SettingsClient>
    {
        public SettingsSessionManager() : base(new SessionClientFactory(), DaisiStaticSettings.DefaultClientKeyProvider, NullLogger.Instance)
        {

        }
        public SettingsSessionManager(SessionClientFactory sessionClientFactory, IClientKeyProvider clientKeyProvider, ILogger<SessionManagerBase<SettingsClient>> logger) : base(sessionClientFactory, clientKeyProvider, logger)
        {
        }
    }
}
