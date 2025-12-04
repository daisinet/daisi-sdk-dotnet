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
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;


namespace Daisi.SDK.Clients.V1.Host
{
    public class InferenceClientFactory(InferenceSessionManager sessionManager)
        : FullyOrchestratedClientFactory<InferenceClient>(sessionManager)
    {
        public InferenceClientFactory(): this(new InferenceSessionManager(new SessionClientFactory(), DaisiStaticSettings.DefaultClientKeyProvider,NullLogger.Instance)) { }
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
            : this(sessionManager, hostIpAddress, hostPort, GrpcChannel.ForAddress($"{(hostIpAddress == DaisiStaticSettings.OrcIpAddressOrDomain ? DaisiStaticSettings.OrcProtocol : "http")}://{hostIpAddress}:{hostPort}"))
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


        #region Close Inference Session
        public async Task<CloseInferenceResponse> CloseAsync(bool closeSession = true)
        {
            var result = await CloseAsync(new CloseInferenceRequest() { InferenceId = InferenceId });

            if (this.SessionManager != null && closeSession)
                await this.SessionManager.CloseAsync();

            return result;
        }
        public override CloseInferenceResponse Close(CloseInferenceRequest request, CallOptions options)
        {
            if (!SessionManager.CheckIsConnected())
            {
                throw new Exception("Client must be connected before closing an inference session.");
            }

            if (this.SessionManager != null && request.SessionId is null)
                request.SessionId = this.SessionManager.SessionId;

            if (string.IsNullOrWhiteSpace(request.InferenceId))
                request.InferenceId = InferenceId;

            var infCreateResponse = SessionManager.UseDirectConnect
                           ? SessionManager.DirectConnectClient.Close(request, options)
                           : base.Close(request, options);


            return infCreateResponse;
        }
        public async new Task<CloseInferenceResponse> CloseAsync(CloseInferenceRequest request, CallOptions options)
        {
            if (!SessionManager.CheckIsConnected())
            {
                throw new Exception("Client must be connected before creating an inference session.");
            }

            if (this.SessionManager != null && string.IsNullOrWhiteSpace(request.SessionId))
                request.SessionId = this.SessionManager.SessionId;

            if (string.IsNullOrWhiteSpace(request.InferenceId))
                request.InferenceId = InferenceId;

            var infCreateResponse = SessionManager.UseDirectConnect
                           ? await SessionManager.DirectConnectClient.CloseAsync(request, options)
                           : await base.CloseAsync(request, options);

            return infCreateResponse;
        }
        #endregion

        #region Create
        public override CreateInferenceResponse Create(CreateInferenceRequest request, CallOptions options)
        {
            if (!SessionManager.CheckIsConnected())
            {
                throw new Exception("Client must be connected before creating an inference session.");
            }

            if (this.SessionManager != null && request.SessionId is null)
                request.SessionId = this.SessionManager.SessionId;

            var infCreateResponse = SessionManager.UseDirectConnect
                           ? SessionManager.DirectConnectClient.Create(request, options)
                           : base.Create(request, options);

            InferenceId = infCreateResponse.InferenceId;
            
            return infCreateResponse;

        }
        public async new Task<CreateInferenceResponse> CreateAsync(CreateInferenceRequest request, CallOptions options)
        {
            if (!SessionManager.CheckIsConnected())
            {
                throw new Exception("Client must be connected before creating an inference session.");
            }

            if (this.SessionManager != null && string.IsNullOrWhiteSpace(request.SessionId))
                request.SessionId = this.SessionManager.SessionId;

            var infCreateResponse = SessionManager.UseDirectConnect
                           ? await SessionManager.DirectConnectClient.CreateAsync(request, options)
                           : await base.CreateAsync(request, options);

            InferenceId = infCreateResponse.InferenceId;

            return infCreateResponse;
        }
        #endregion

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
        

        public AsyncServerStreamingCall<SendInferenceResponse> Send(string userInputText)
        {
            var request = SendInferenceRequest.CreateDefault();
            request.Text = userInputText;
            return Send(request);
        }
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
