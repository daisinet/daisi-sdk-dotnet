using Daisi.Protos.V1;
using Daisi.SDK.Clients.V1.SessionManagers;
using Daisi.SDK.Interfaces;
using Daisi.SDK.Models;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.SDK.Clients.V1.Host
{
    public class SettingsClientFactory(SettingsSessionManager sessionManager)
        : FullyOrchestratedClientFactory<SettingsClient>(sessionManager)
    {
        public SettingsClientFactory()
            : this(new SettingsSessionManager(new Orc.SessionClientFactory(), 
                DaisiStaticSettings.DefaultClientKeyProvider, NullLogger.Instance)) 
        { 
        
        }
        public override SettingsClient Create(string? orcDomainOrIp = null, int? orcPort = null)
        {
            throw new Exception("You cannot create a SettingsClient without providing context to the specific Host. Use Create(hostId).");
        }
        public override SettingsClient Create(string hostIpAddress, int hostPort, string? orcDomainOrIp = null, int? orcPort = null)
        {
            throw new Exception("You cannot create a SettingsClient without providing context to the specific Host. Use Create(hostId).");
        }
    }
    public class SettingsClient : SettingsProto.SettingsProtoClient
    {
        public SettingsSessionManager SessionManager { get; }

        public SettingsClient(SettingsSessionManager sessionManager, string hostId)
         : base(GrpcChannel.ForAddress(DaisiStaticSettings.OrcUrl)
               .Intercept((metadata) =>
                 {
                     if (sessionManager.ClientKeyProvider is not null)
                         metadata.Add(DaisiStaticSettings.ClientKeyHeader, sessionManager.ClientKeyProvider.GetClientKey());
                     else
                         metadata.Add(DaisiStaticSettings.ClientKeyHeader, DaisiStaticSettings.ClientKey);

                     return metadata;
                 }))
        {
            this.SessionManager = sessionManager;
            this.SessionManager.NegotiateSession(new CreateSessionRequest() { HostId = hostId });
        }
            
            
        public SettingsClient(SettingsSessionManager sessionManager, string hostIpAddress, int port)
            : this(sessionManager, GrpcChannel.ForAddress($"http://{hostIpAddress}:{port}"))
        { }
        public SettingsClient(SettingsSessionManager sessionManager, GrpcChannel connectionChannel) 
            : base(connectionChannel.Intercept((metadata) =>
            {
                if (sessionManager.ClientKeyProvider is not null)
                    metadata.Add(DaisiStaticSettings.ClientKeyHeader, sessionManager.ClientKeyProvider.GetClientKey());
                else
                    metadata.Add(DaisiStaticSettings.ClientKeyHeader, DaisiStaticSettings.ClientKey);

                return metadata;
            })) { 
            this.SessionManager = sessionManager;
            this.SessionManager.NegotiateSession();
        }
        public async Task CloseAsync()
        {
            if (this.SessionManager != null)
            {
                await this.SessionManager.CloseAsync();
            }
        }
        protected internal GetAllSettingsResponse GetAll(CallOptions options) => GetAll(new GetAllSettingsRequest(), options);
        public GetAllSettingsResponse GetAll() => GetAll(new CallOptions());
        public override GetAllSettingsResponse GetAll(GetAllSettingsRequest request, CallOptions options)
        {
            if (!SessionManager.CheckIsConnected())
            {
                throw new Exception("Client must be connected.");
            }

            if (string.IsNullOrWhiteSpace(request.SessionId))
                request.SessionId = SessionManager.SessionId;

            var response = SessionManager.UseDirectConnect
                            ? SessionManager.DirectConnectClient.GetAll(request, options)
                            : base.GetAll(request, options);

            return response;
        }
        public override AsyncUnaryCall<GetAllSettingsResponse> GetAllAsync(GetAllSettingsRequest request, CallOptions options)
        {
            if (!SessionManager.CheckIsConnected())
            {
                throw new Exception("Client must be connected.");
            }

            if (string.IsNullOrWhiteSpace(request.SessionId))
                request.SessionId = SessionManager.SessionId;

            var response = SessionManager.UseDirectConnect
                            ? SessionManager.DirectConnectClient.GetAllAsync(request, options)
                            : base.GetAllAsync(request, options);

            return response;
        }

        protected internal AsyncUnaryCall<GetAllSettingsResponse> GetAllAsync(CallOptions options) => GetAllAsync(new(), options);
        public AsyncUnaryCall<GetAllSettingsResponse> GetAllAsync() => GetAllAsync(new());

        public override SetAllSettingsResponse SetAll(SetAllSettingsRequest request, CallOptions options)
        {
            if (!SessionManager.CheckIsConnected())
            {
                throw new Exception("Client must be connected.");
            }                     

            if (string.IsNullOrWhiteSpace(request.SessionId))
                request.SessionId = SessionManager.SessionId;

            var response = SessionManager.UseDirectConnect
                            ? SessionManager.DirectConnectClient.SetAll(request, options)
                            : base.SetAll(request, options);

            return response;
        }
        public override AsyncUnaryCall<SetAllSettingsResponse> SetAllAsync(SetAllSettingsRequest request, CallOptions options)
        {

            if (!SessionManager.CheckIsConnected())
            {
                throw new Exception("Client must be connected.");
            }

            if (string.IsNullOrWhiteSpace(request.SessionId))
                request.SessionId = SessionManager.SessionId;

            var response = SessionManager.UseDirectConnect
                            ? SessionManager.DirectConnectClient.SetAllAsync(request, options)
                            : base.SetAllAsync(request, options);

            return response;
        }
    }
}
