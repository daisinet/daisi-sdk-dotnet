using Daisi.SDK.Interfaces.Authentication;

namespace Daisi.SDK.Providers;

public interface IDriveIdentityProvider : IClientKeyProvider
{
    string GetAccountId();
    string GetUserId();
    string GetUserName();
    int GetUserRole();
}
