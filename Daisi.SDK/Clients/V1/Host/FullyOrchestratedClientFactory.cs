using Daisi.Protos.V1;
using Daisi.SDK.Clients.V1.Orc;
using Daisi.SDK.Clients.V1.SessionManagers;
using Daisi.SDK.Interfaces;
using Daisi.SDK.Models;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Daisi.SDK.Clients.V1.Host
{
    public abstract class FullyOrchestratedClientFactory<T>(SessionManagerBase<T> sessionManager)
        where T : class
    {

        /// <summary>
        /// Goes to the Orc and determines the Host to use. Then it determines if
        /// the connection needs to be relayed or uses Direct Connect to the Host.
        /// </summary>
        /// <param name="orcDomainOrIp"></param>
        /// <param name="orcPort"></param>
        /// <returns></returns>
        public virtual T Create(string? orcDomainOrIp = default, int? orcPort = null)
          => Create(orcDomainOrIp ?? DaisiStaticSettings.OrcIpAddressOrDomain,
              orcPort ?? DaisiStaticSettings.OrcPort,
              orcDomainOrIp ?? DaisiStaticSettings.OrcIpAddressOrDomain,
              orcPort ?? DaisiStaticSettings.OrcPort);

        /// <summary>
        /// Creates a client that directly reaches out to a Host after authenticating with Orc.
        /// Usually, you will want the Orc to determine which Host to use, but in custom setups, you may want to
        /// reach out to a Host directly.
        /// </summary>
        /// <param name="hostIpAddress">The IP Address or Domain for the Host.</param>
        /// <param name="hostPort">The port that the Host uses.</param>
        /// <param name="orcDomainOrIp">The Ip Address or Domain that the Orc uses.</param>
        /// <param name="orcPort">The port that the Orc uses.</param>
        /// <returns></returns>
        public virtual T Create(string hostIpAddress, int hostPort,
            string? orcDomainOrIp = default, int? orcPort = null)
        {
            var newSessionManager = sessionManager.CreateNewInstance();
            var client = (T)Activator.CreateInstance(typeof(T), BindingFlags.NonPublic | BindingFlags.Instance , binder: null, args:[ newSessionManager, hostIpAddress, hostPort], culture:null )!;
            return client;
        }

        /// <summary>
        /// Creates a client that goes to the orc, but sets up the session with the Host specified.
        /// </summary>
        /// <param name="hostId">Must be a host ID that belongs to the account.</param>
        /// <returns></returns>
        public virtual T Create(string hostId)
        {
            var newSessionManager = sessionManager.CreateNewInstance(hostId);
            var client = (T)Activator.CreateInstance(typeof(T), BindingFlags.NonPublic | BindingFlags.Instance, null, args:[newSessionManager, hostId], null)!;
            return client;
        }

        public virtual void Reset()
        {
            sessionManager = sessionManager.CreateNewInstance();
        }
        
    }
    
}
