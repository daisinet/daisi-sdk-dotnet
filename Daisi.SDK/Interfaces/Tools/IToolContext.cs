using Daisi.Protos.V1;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.SDK.Interfaces.Tools
{
    /// <summary>
    /// Used to give a tool the ability to understand the context in which it is running.
    /// </summary>
    public interface IToolContext
    {

        /// <summary>
        /// Runs inference on the provided text in the current context.
        /// </summary>
        /// <param name="request">The inteference request that should be run through the AIModel in the current context.</param>
        /// <returns>The result of the inference.</returns>
        Task<SendInferenceResponse> InferAsync(SendInferenceRequest request);

        /// <summary>
        /// Services available to the tool in the current context.
        /// </summary>
        IServiceProvider Services { get; }

        /// <summary>
        /// The session ID for the current inference session, used for secure tool validation.
        /// </summary>
        string? SessionId { get; }
    }
}
