using Daisi.Protos.V1;
using Daisi.SDK.Clients.V1.Orc;
using Daisi.SDK.Clients.V1.SessionManagers;
using Daisi.SDK.Interfaces;
using Daisi.SDK.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Core.Utils;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;


namespace Daisi.SDK.Clients.V1.Host
{
    public class InferenceClientFactory(InferenceSessionManager sessionManager)
        : FullyOrchestratedClientFactory<InferenceClient>(sessionManager)
    {
     
    }
    public partial class InferenceClient : InferencesProto.InferencesProtoClient
    {
        public InferenceSessionManager SessionManager { get; }
        public string InferenceId { get; set;  }

        public InferenceClient(InferenceSessionManager sessionManager, string hostId)
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

        public InferenceClient(InferenceSessionManager sessionManager,string hostIpAddress, int hostPort)
            : this(sessionManager, hostIpAddress, hostPort, GrpcChannel.ForAddress($"http://{hostIpAddress}:{hostPort}"))
        {}

        public InferenceClient(InferenceSessionManager sessionManager, string hostIpAddress, int port, GrpcChannel orcChannel)
            : base(orcChannel.Intercept((metadata) =>
         {
             if (sessionManager.ClientKeyProvider is not null)
                 metadata.Add(DaisiStaticSettings.ClientKeyHeader, sessionManager.ClientKeyProvider.GetClientKey());
             else
                 metadata.Add(DaisiStaticSettings.ClientKeyHeader, DaisiStaticSettings.ClientKey);

             return metadata;
         }))
        {
            this.SessionManager = sessionManager;
            this.SessionManager.NegotiateSession();            
        }

        public async Task CloseAsync()
        {
            if(this.SessionManager != null)
            {
                await this.SessionManager.CloseAsync();
            }
        }

        #region Stats
        public override InferenceStatsResponse Stats(InferenceStatsRequest request, CallOptions options)
        {
            if (!SessionManager.CheckIsConnected())
            {
                throw new Exception("Client must be connected before getting stats.");
            }

            if (string.IsNullOrWhiteSpace(request.SessionId))
                request.SessionId = SessionManager.SessionId;

            if (string.IsNullOrWhiteSpace(request.InferenceId))
            {
                if (string.IsNullOrWhiteSpace(request.InferenceId))
                {
                    // Inference hasn't started, so just return an empty response and save going to the host.
                    return new InferenceStatsResponse() { Success = true };
                }

                request.InferenceId = InferenceId;
            }

            var response = SessionManager.UseDirectConnect
                             ? SessionManager.DirectConnectClient.Stats(request, options)
                             : base.Stats(request, options);
            return response;
        }
        public override AsyncUnaryCall<InferenceStatsResponse> StatsAsync(InferenceStatsRequest request, CallOptions options)
        {
            if (!SessionManager.CheckIsConnected())
            {
                throw new Exception("Client must be connected before getting stats.");
            }

            if (string.IsNullOrWhiteSpace(request.SessionId))
                request.SessionId = SessionManager.SessionId;

            if (string.IsNullOrWhiteSpace(request.InferenceId))
            {
                if(string.IsNullOrWhiteSpace(InferenceId))
                {
                    // Inference hasn't started, so just return an empty response and save going to the host.
                    return new AsyncUnaryCall<InferenceStatsResponse>(Task.FromResult(new InferenceStatsResponse() { Success = true }), Task.FromResult(new Metadata()), ()=> new Status(), ()=>new Metadata(), () => { });                   
                }

                request.InferenceId = InferenceId;
            }

            var response = SessionManager.UseDirectConnect
                             ? SessionManager.DirectConnectClient.StatsAsync(request, options)
                             : base.StatsAsync(request, options);


            return response;
        }

        #endregion

        #region Send
        public override AsyncServerStreamingCall<SendInferenceResponse> Send(SendInferenceRequest request, CallOptions options)
        {
            if (!SessionManager.CheckIsConnected())
            {
                throw new Exception("Client must be connected before sending an inference.");
            }

            if(string.IsNullOrWhiteSpace(request.SessionId))
                request.SessionId = SessionManager.SessionId;

            if (string.IsNullOrEmpty(InferenceId) && string.IsNullOrWhiteSpace(request.InferenceId))
            {
                var infCreateRequest = new CreateInferenceRequest()
                {
                    SessionId = SessionManager.SessionId                    
                };

                var infCreateResponse = SessionManager.UseDirectConnect
                            ? SessionManager.DirectConnectClient.Create(infCreateRequest, options)
                            : base.Create(infCreateRequest, options);
                
                InferenceId = infCreateResponse.InferenceId;
               
            }

            if (string.IsNullOrWhiteSpace(request.InferenceId))
            {
                request.InferenceId = InferenceId;
            }

            var response = SessionManager.UseDirectConnect
                            ? SessionManager.DirectConnectClient.Send(request, options)
                            : base.Send(request, options);
           
            
            return response;
        }        

        #endregion
    }
}
