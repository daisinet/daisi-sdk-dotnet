using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Daisi.Protos.V1
{
    public partial class InferenceStatsResponse
    {
        [JsonIgnore]
        public double LastMessageTokensPerSecond
        {
            get
            {
                if (LastMessageComputeTimeMs == 0)
                {
                    return 0;
                }
                return LastMessageTokenCount / (LastMessageComputeTimeMs / 1000.0);
            }
        }

        [JsonIgnore]
        public double SessionTokensPerSecond
        {
            get
            {
                if (SessionComputeTimeMs == 0)
                {
                    return 0;
                }
                return SessionTokenCount / (SessionComputeTimeMs / 1000.0);
            }
        }
    }
}
