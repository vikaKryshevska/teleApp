using Microsoft.AspNetCore.Identity;

namespace ApiGateway.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(IdentityUser user, String role);
    }
}
