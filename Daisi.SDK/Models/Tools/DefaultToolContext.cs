using Daisi.Protos.V1;
using Daisi.SDK.Interfaces.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.SDK.Models.Tools
{
    public class DefaultToolContext : IToolContext
    {
        public IDaisiTool[] AvailableTools { get; set; }

        public IServiceProvider Services { get; set; }

        Func<SendInferenceRequest, Task<SendInferenceResponse>> LocalInferenceCallback;

        public DefaultToolContext(Func<SendInferenceRequest, Task<SendInferenceResponse>> localInferenceCallback)
        {
            LocalInferenceCallback = localInferenceCallback;
        }
        public async Task<SendInferenceResponse> InferAsync(SendInferenceRequest request)
            => await LocalInferenceCallback(request);
    }
}
