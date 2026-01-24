using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.Protos.V1
{
    public partial class GetHostsRequest
    {
        partial void OnConstruction()
        {
            this.Paging = new();
        }
    }
}
