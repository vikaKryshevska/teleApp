using Microsoft.AspNetCore.Identity;

namespace TelephoneServices.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(IdentityUser user, String role);
    }
}
