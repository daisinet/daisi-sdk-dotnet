using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.Protos.V1
{
    public partial class BackendSettings
    {
        partial void OnConstruction()
        {
            this.ContextSize = 8192;
            this.GpuLayerCount = -1;
            this.BatchSize = 128;
        }
    }
}
