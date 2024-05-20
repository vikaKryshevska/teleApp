using Microsoft.AspNetCore.Identity;

namespace UserService.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(IdentityUser user, String role);
    }
}
