using System.Security.Claims;

namespace TetaBackend.Features.User.Interfaces;

public interface IJwtService
{
    string GenerateToken(Guid userId);
}