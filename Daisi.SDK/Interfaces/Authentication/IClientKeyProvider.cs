using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.SDK.Interfaces.Authentication
{
    public interface IClientKeyProvider
    {
        string GetClientKey();
    }
}
