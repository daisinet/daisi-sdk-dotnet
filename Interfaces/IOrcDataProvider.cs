using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.SDK.Interfaces
{
    public interface IOrcDataProvider
    {
        string GetOrcIpAddressOrDomain();
        int GetOrcPort();
    }
}
