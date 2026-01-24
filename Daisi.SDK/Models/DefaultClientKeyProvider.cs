using Daisi.Protos.V1;
using Daisi.SDK.Clients.V1.Orc;
using Daisi.SDK.Interfaces.Authentication;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.SDK.Models
{
    public class DefaultClientKeyProvider : IClientKeyProvider
    {       
        public string GetClientKey()
        {
            return DaisiStaticSettings.ClientKey;
        }
    }
}
