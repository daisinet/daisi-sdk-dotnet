using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.Orc.Core.Data.Models
{
    public class RPCStatuses
    {
        public static Status InvalidAccount = new Status(StatusCode.InvalidArgument, "Invalid Account");
    }
}
