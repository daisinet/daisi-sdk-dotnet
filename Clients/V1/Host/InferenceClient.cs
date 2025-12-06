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
    public class InferenceClientFactory : FullyOrchestratedClientFactory<InferenceClient>
    {
        public InferenceClientFactory(InferenceSessionManager sessionManager) : base(sessionManager)
        {
        }
        public InferenceClientFactory() : base(new InferenceSessionManager()) { }
    }
    public partial class InferenceClient : InferencesProto.InferencesProtoClient
    {
        /// <summary>
        /// This manages the Orc Session with the Orc that is required to stay alive
        /// for valid communication with any Host, whether using DC or FOC.
        /// </summary>
        public InferenceSessionManager SessionManager { get; }

        /// <summary>
        /// Gets and sets the ID for the Inference Session.
        /// </summary>
        public string InferenceId { get; set; }

        internal InferenceClient(InferenceSessionManager sessionManager, string hostId)
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

        internal InferenceClient(InferenceSessionManager sessionManager, string hostIpAddress, int hostPort)
            : this(sessionManager, hostIpAddress, hostPort, GrpcChannel.ForAddress($"{(hostIpAddress == DaisiStaticSettings.OrcIpAddressOrDomain ? DaisiStaticSettings.OrcProtocol : "http")}://{hostIpAddress}:{hostPort}"))
        { }

        internal InferenceClient(InferenceSessionManager sessionManager, string hostIpAddress, int port, GrpcChannel orcChannel)
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

        /// <summary>
        /// Closes the inference session and optionally the Orc Session as well.
        /// </summary>
        /// <param name="closeOrcSession">If true, it closes out of the Orc Session completely. Default is true.</param>
        /// <returns>The Close response from the Orc for FOC or the response from the Host in DC.</returns>
        public async Task<CloseInferenceResponse> CloseAsync(bool closeOrcSession = true)
        {
            var result = await CloseAsync(new CloseInferenceRequest() { SessionId = SessionManager.SessionId, InferenceId = InferenceId });

            if (this.SessionManager != null && closeOrcSession)
                await this.SessionManager.CloseAsync();

            return result;
        }

        /// <summary>
        /// Closes the inference session but always leaves the Orc Session open.
        /// You probably want to use the CloseAsync(closeOrcSession) overload.
        /// </summary>
        /// <param name="request">Details for which Inference Session to close.</param>
        /// <param name="options">Grpc call options.</param>
        /// <returns>The Close Inference Response from the Orc for FOC or the response from the Host in DC.</returns>
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

            InferenceId = null;

            return infCreateResponse;
        }

        /// <summary>
        /// Closes the inference session but always leaves the Orc Session open.
        /// NOTE: You probably want to use the CloseAsync(bool closeOrcSession) overload instead.
        /// </summary>
        /// <param name="request">Details for which Inference Session to close.</param>
        /// <param name="options">Grpc call options.</param>
        /// <returns>The Close Inference Response from the Orc for FOC or the response from the Host in DC.</returns>
        public override AsyncUnaryCall<CloseInferenceResponse> CloseAsync(CloseInferenceRequest request, CallOptions options)
        {
            if (!SessionManager.CheckIsConnected())
            {
                throw new Exception("Client must be connected before creating an inference session.");
            }

            if (this.SessionManager != null && string.IsNullOrWhiteSpace(request.SessionId))
                request.SessionId = this.SessionManager.SessionId;

            if (string.IsNullOrWhiteSpace(request.InferenceId))
                request.InferenceId = InferenceId;

            var infCloseResponse = SessionManager.UseDirectConnect
                           ? SessionManager.DirectConnectClient.CloseAsync(request, options)
                           : base.CloseAsync(request, options);

            var wrappedResponseAsync = infCloseResponse.ResponseAsync.ContinueWith<CloseInferenceResponse>(t =>
            {
                if (t.IsCanceled)
                    throw new TaskCanceledException(t);

                var response = t.Result;
                if (response.Success)
                {
                    InferenceId = default!;
                }

                return response;
            }, TaskContinuationOptions.ExecuteSynchronously);

            var wrappedHeadersAsync = infCloseResponse.ResponseHeadersAsync;
            var wrappedTrailers = infCloseResponse.GetTrailers;
            var wrappedStatus = infCloseResponse.GetStatus;
            var wrappedDispose = infCloseResponse.Dispose;

            return new AsyncUnaryCall<CloseInferenceResponse>(
                wrappedResponseAsync,
                wrappedHeadersAsync,
                wrappedStatus,
                wrappedTrailers,
                wrappedDispose);
        }
        #endregion

        #region Create

        /// <summary>
        /// Creates an Inference Session with a host after successfully creating a session.
        /// </summary>
        /// <param name="request">The request criteria for the Inference Session</param>
        /// <param name="headers">The headers that are to be passed via http/2 to the Host or Orc.</param>
        /// <param name="deadline">When does this request die?</param>
        /// <param name="cancellationToken">Listens for stop requests and pulls out of the call.</param>
        /// <returns>CreateInferenceResponse which contains information regarding the new Inference Session.</returns>
        public override CreateInferenceResponse Create(CreateInferenceRequest request, Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
        {
            return base.Create(request, headers, deadline, cancellationToken);
        }

        /// <summary>
        /// Creates an Inference Session with a host after successfully creating a session.
        /// </summary>
        /// <param name="request">The request criteria for the Inference Session</param>
        /// <param name="headers">The headers that are to be passed via http/2 to the Host or Orc.</param>
        /// <param name="deadline">When does this request die?</param>
        /// <param name="cancellationToken">Listens for stop requests and pulls out of the call.</param>
        /// <returns>CreateInferenceResponse which contains information regarding the new Inference Session.</returns>
        public override AsyncUnaryCall<CreateInferenceResponse> CreateAsync(CreateInferenceRequest request, Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
        {
            return base.CreateAsync(request, headers, deadline, cancellationToken);
        }

        /// <summary>
        /// Creates an Inference Session with a host after successfully creating a session.
        /// </summary>
        /// <param name="request">The request criteria for the Inference Session</param>
        /// <param name="options">The Grpc call options.</param>
        /// <returns>CreateInferenceResponse which contains information regarding the new Inference Session.</returns>
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

        /// <summary>
        /// Creates an Inference Session with a host after successfully creating a session.
        /// </summary>
        /// <param name="request">The request criteria for the Inference Session</param>
        /// <param name="options">The Grpc call options.</param>
        /// <returns>CreateInferenceResponse which contains information regarding the new Inference Session.</returns>
        public override AsyncUnaryCall<CreateInferenceResponse> CreateAsync(CreateInferenceRequest request, CallOptions options)
        {
            if (!SessionManager.CheckIsConnected())
            {
                throw new Exception("Client must be connected before creating an inference session.");
            }

            if (this.SessionManager != null && string.IsNullOrWhiteSpace(request.SessionId))
                request.SessionId = this.SessionManager.SessionId;

            var baseCall = SessionManager.UseDirectConnect
                          ? SessionManager.DirectConnectClient.CreateAsync(request, options)
                          : base.CreateAsync(request, options);

            var wrappedResponseAsync = baseCall.ResponseAsync.ContinueWith<CreateInferenceResponse>(t =>
            {
                if (t.IsCanceled)
                    throw new TaskCanceledException(t);

                var response = t.Result;

                InferenceId = response.InferenceId;
                
                return response;
            }, TaskContinuationOptions.ExecuteSynchronously);

            var wrappedHeadersAsync = baseCall.ResponseHeadersAsync;
            var wrappedTrailers = baseCall.GetTrailers;
            var wrappedStatus = baseCall.GetStatus;
            var wrappedDispose = baseCall.Dispose;

            return new AsyncUnaryCall<CreateInferenceResponse>(
                wrappedResponseAsync,
                wrappedHeadersAsync,
                wrappedStatus,
                wrappedTrailers,
                wrappedDispose);
        }
        #endregion

        #region Stats
        /// <summary>
        /// Gets the token counts, process times, and tool counts for the current Inference Session as well as the last 
        /// SendInferenceRequest that was called, if any.
        /// </summary>
        /// <param name="request">The relavant information about which Inference and Orc Session. InferenceId and SessionId will be added automatically if blank.</param>
        /// <param name="options">The Grpc call options.</param>
        /// <returns>Statistics regarding the last request and whole session.</returns>
        /// <exception cref="Exception">Throughs an exception if the client has not successfully created a session with the Orc.</exception>
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

        /// <summary>
        /// Gets the token counts, process times, and tool counts for the current Inference Session as well as the last 
        /// SendInferenceRequest that was called, if any.
        /// </summary>
        /// <param name="request">The relavant information about which Inference and Orc Session. InferenceId and SessionId will be added automatically if blank.</param>
        /// <param name="options">The Grpc call options.</param>
        /// <returns>Statistics regarding the last request and whole session.</returns>
        /// <exception cref="Exception">Throughs an exception if the client has not successfully created a session with the Orc.</exception>
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
                if (string.IsNullOrWhiteSpace(InferenceId))
                {
                    // Inference hasn't started, so just return an empty response and save going to the host.
                    return new AsyncUnaryCall<InferenceStatsResponse>(Task.FromResult(new InferenceStatsResponse() { Success = true }), Task.FromResult(new Metadata()), () => new Status(), () => new Metadata(), () => { });
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

        /// <summary>
        /// Creates a default SendInferenceRequest and sets the userInputText appropriately.
        /// Send the request to the Host (DC) or Orc (FOC).
        /// </summary>
        /// <param name="userInputText">The text that you want processed by the model.</param>
        /// <returns>An async stream that allows for getting each token one at a time.</returns>
        public AsyncServerStreamingCall<SendInferenceResponse> Send(string userInputText)
        {
            var request = SendInferenceRequest.CreateDefault();
            request.Text = userInputText;
            return Send(request);
        }

        /// <summary>
        /// Sends a request to the Host (DC) or Orc (FOC).
        /// </summary>
        /// <param name="request">The text that you want processed by the model. SessionId and InferenceId will be set if left blank.</param>
        /// <param name="options">Grpc options for the call.</param>
        /// <returns>An async stream that allows for getting each token one at a time.</returns>
        public override AsyncServerStreamingCall<SendInferenceResponse> Send(SendInferenceRequest request, CallOptions options)
        {
            if (!SessionManager.CheckIsConnected())
            {
                throw new Exception("Client must be connected before sending an inference.");
            }

            if (string.IsNullOrWhiteSpace(request.SessionId))
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

        /// <summary>
        /// Sends a request to the Host (DC) or Orc (FOC).
        /// </summary>
        /// <param name="request">The text that you want processed by the model. SessionId and InferenceId will be set if left blank.</param>
        /// <param name="headers">The headers that are to be passed via http/2 to the Host or Orc.</param>
        /// <param name="deadline">When does this request die?</param>
        /// <param name="cancellationToken">Listens for stop requests and pulls out of the call.</param>
        /// <returns>An async stream that allows for getting each token one at a time.</returns>
        public override AsyncServerStreamingCall<SendInferenceResponse> Send(SendInferenceRequest request, Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
        {
            return base.Send(request, headers, deadline, cancellationToken);
        }

        #endregion
    }
}
