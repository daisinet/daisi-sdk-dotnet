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
                AntiPrompts = { "User:", "User:\n", "\n\n\n", "###" },
                DecodeSpecialTokens = false,
                FrequencyPenalty = 0.0f,
                MaxTokens = 32000,
                MinKeep = 1,
                MinP = 0.1f,
                PenalizeNewline = false,
                PenaltyCount = 64,
                PresencePenalty = 0.0f,
                PreventEOS = false,
                RepeatPenalty = 1.1f,
                Seed = Random.Shared.Next(),
                Temperature = 0.7f,                
                TokensKeep = 16000,                
                TopK = 40,
                TypicalP = 1f,
                TopP = 0.9f,
                ThinkLevel = ThinkLevels.Basic
            };
        }
    }
}
