using Daisi.Protos.V1;
using Daisi.SDK.Interfaces.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.SDK.Models.Tools
{
    public class DefaultToolContext : IToolContext
    {

        public IServiceProvider Services => DaisiStaticSettings.Services;

        public string? SessionId { get; }

        Func<SendInferenceRequest, Task<SendInferenceResponse>> LocalInferenceCallback;

        public DefaultToolContext(Func<SendInferenceRequest, Task<SendInferenceResponse>> localInferenceCallback, string? sessionId = null)
        {
            LocalInferenceCallback = localInferenceCallback;
            SessionId = sessionId;
        }
        public async Task<SendInferenceResponse> InferAsync(SendInferenceRequest request)
            => await LocalInferenceCallback(request);


    }
}
