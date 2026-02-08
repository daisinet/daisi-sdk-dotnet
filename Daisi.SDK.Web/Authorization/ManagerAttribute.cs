using Daisi.Protos.V1;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.SDK.Web.Authorization
{
    public class ManagerAttribute : AuthorizeAttribute
    {
        public const string DaisiManagerPolicyName = $"DAISI-MANAGER";
        public ManagerAttribute() {
            Policy = DaisiManagerPolicyName;
        }
    }

    public class OwnerAttribute : AuthorizeAttribute
    {
        public const string DaisiOwnerPolicyName = $"DAISI-OWNER";
        public OwnerAttribute()
        {
            Policy = DaisiOwnerPolicyName;
        }
    }
}
