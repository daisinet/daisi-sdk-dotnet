using Daisi.Protos.V1;
using Daisi.SDK.Clients.V1.Orc;
using Daisi.SDK.Interfaces.Authentication;
using Daisi.SDK.Models;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Daisi.SDK.Clients.V1.SessionManagers
{
    /// <summary>
    /// The SessionManager makes sure the DaisiSession is opened properly with the Orc.
    /// All communications with the Host needs an active session with the Orc, even when Direct Connect is active.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SessionManagerBase<T>
    {
        ILogger logger { get; set; }
        AsyncUnaryCall<ConnectResponse>? asyncConnectResponse;
        ConnectResponse? connectResponse;

        /// <summary>
        /// Limits the session to calls to this host.
        /// </summary>
        public string HostId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the current session as provided by the Orc.
        /// </summary>
        public string? SessionId { get; set; }

        /// <summary>
        /// Gets whether the Session should use Direct Connect to connect with the Host.
        /// </summary>
        public bool UseDirectConnect { get => DirectConnectClient is not null; }

        /// <summary>
        /// The client that will be used if UseDirectConnect is true.
        /// </summary>
        public T? DirectConnectClient { get; set; }

        /// <summary>
        /// The Client Key Provider to use for authentication when making client calls.
        /// </summary>
        public IClientKeyProvider ClientKeyProvider { get; set; }

        /// <summary>
        /// The SessionClient that will communicate with the Orc.
        /// </summary>
        public SessionClient SessionClient { get; private set; }

        SessionClientFactory SessionClientFactory { get; set; }
        
        public SessionManagerBase(SessionClientFactory sessionClientFactory, IClientKeyProvider clientKeyProvider, ILogger logger)
        {
            SessionClientFactory = sessionClientFactory;

            ClientKeyProvider = clientKeyProvider;
            SessionClient = sessionClientFactory.Create();
            this.logger = logger;
        }

        void TryLogInfo(string text)
        {
            if (logger is null) return;
            logger.LogInformation(text);
        }
        public virtual SessionManagerBase<T> CreateNewInstance(string? hostId = null)
        {
            var newInstance = (SessionManagerBase<T>)Activator.CreateInstance(this.GetType(), new SessionClientFactory(ClientKeyProvider), ClientKeyProvider, logger);
            newInstance.HostId = hostId;
            return newInstance;
        }
        public void NegotiateSession(CreateSessionRequest? createSessionRequest = default)
        {
            if (this.SessionId is not null)
                return;

            if (HostId != createSessionRequest?.HostId)
                throw new Exception($"Client was created for Host {HostId} and cannot be used to send messages to Host {createSessionRequest.HostId}. You must create a new instance of the client.");

            createSessionRequest ??= new CreateSessionRequest();
            createSessionRequest.NetworkName ??= DaisiStaticSettings.NetworkName;

            TryLogInfo("Negotiating Session... ");
            var sessionResponse = SessionClient.Create(createSessionRequest);

            if (!sessionResponse.Success)
            {
                if(sessionResponse.MoveToOrc is not null)
                {
                    TryLogInfo($"Moving to Orc: {sessionResponse.MoveToOrc.Name}");
                    DaisiStaticSettings.OrcIpAddressOrDomain = sessionResponse.MoveToOrc.Domain;
                    DaisiStaticSettings.OrcPort = sessionResponse.MoveToOrc.Port;

                    SessionClient = SessionClientFactory.Create();
                    NegotiateSession(createSessionRequest);
                    return;
                }
            }

            this.SessionId = sessionResponse.Id;
            TryLogInfo($"Session Created {sessionResponse.Id}");

            TryLogInfo("Connecting To Session... ");
            var connectResponse = Connect(new ConnectRequest() { SessionId = this.SessionId });

            if (!CheckIsConnected())
            {
                throw new Exception("Could not connect.");
            }

            if (sessionResponse.Host.DirectConnect)
            {
                TryLogInfo($"Connected with Direct Connection enabled");
                var dc = (SessionManagerBase<T>)this.MemberwiseClone();
                DirectConnectClient = (T)Activator.CreateInstance(typeof(T), BindingFlags.NonPublic | BindingFlags.Instance, binder: null, args: [dc, sessionResponse.Host.IpAddress, sessionResponse.Host.Port], culture: null)!;
            }
            else
                TryLogInfo($"Connected with Fully Orchestrated Connection enabled");


        }

        public async Task CloseAsync()
        {
            TryLogInfo($"Closing Session: {SessionId}");
            if (SessionClient is not null)
            {
                await SessionClient.CloseAsync(new CloseSessionRequest() { Id = this.SessionId });
                asyncConnectResponse = null;
                connectResponse = null;
                SessionId = null;
            }
            TryLogInfo($"Closed Session");

        }

        #region Connect
        public ConnectResponse Connect(ConnectRequest request, CallOptions options)
        {
            var resp = SessionClient.Connect(request, options);
            connectResponse = resp;
            return resp;
        }
        public ConnectResponse Connect(ConnectRequest request, Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
        {
            var resp = SessionClient.Connect(request, headers, deadline, cancellationToken);
            connectResponse = resp;
            return resp;
        }
        public AsyncUnaryCall<ConnectResponse> ConnectAsync(ConnectRequest request, CallOptions options)
        {
            var resp = SessionClient.ConnectAsync(request, options);
            asyncConnectResponse = resp;
            return resp;
        }
        public AsyncUnaryCall<ConnectResponse> ConnectAsync(ConnectRequest request, Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
        {
            var resp = SessionClient.ConnectAsync(request, headers, deadline, cancellationToken);
            asyncConnectResponse = resp;
            return resp;
        }
        public bool CheckIsConnected()
        {
            return connectResponse is not null && !string.IsNullOrWhiteSpace(connectResponse.Id);
        }
        public async Task<bool> CheckIsConnectedAsync()
        {
            return CheckIsConnected() || (asyncConnectResponse is not null && !string.IsNullOrWhiteSpace((await asyncConnectResponse.ResponseAsync).Id));
        }
        #endregion
    }
}
