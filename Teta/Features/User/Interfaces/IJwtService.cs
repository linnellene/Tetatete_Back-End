using System.IdentityModel.Tokens.Jwt;

namespace TetaBackend.Features.User.Interfaces;

public interface IJwtService
{
    string GenerateToken(Guid userId, string email);

    JwtSecurityToken? ValidateAndDecodeJwtToken(string token);
}