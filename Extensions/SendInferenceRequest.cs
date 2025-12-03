using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.Protos.V1
{
    public partial class SendInferenceRequest
    {
        public static SendInferenceRequest CreateDefault()
        {
            return new SendInferenceRequest
            {
                TypicalP = 1f,
                TopK = 40,
                TopP = 0.9f,
                Temperature = 0.75f,                
                MaxTokens = 4096,
                RepeatPenalty = 1.1f,
                Seed = Random.Shared.Next(),
                DecodeSpecialTokens = false,
                PreventEOS = false,
                MinKeep = 1,
                MinP = 0.1f,
                PenalizeNewline = false,
                PenaltyCount = 64,
                PresencePenalty = 0.0f,
                FrequencyPenalty = 0.0f,
                AntiPrompts = { "User:", "User:\n", "\n\n\n", "###" },
                TokensKeep = 2048                
            };
        }
    }
}
