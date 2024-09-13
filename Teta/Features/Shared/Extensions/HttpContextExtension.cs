using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TetaBackend.Features.User.Interfaces;

namespace TetaBackend.Features.Shared.Extentions;

public static class HttpContextExtension
{
    public static string? GetUserIdFromJwt(this HttpContext context, IJwtService jwtService)
    {
        var authorizationHeader = context.Request.Headers.Authorization;

        if (!authorizationHeader.ToString().StartsWith("Bearer"))
        {
            return null;
        }

        var token = authorizationHeader.ToString().Substring("Bearer ".Length).Trim();
        
        var jwt = jwtService.ValidateAndDecodeJwtToken(token);

        return jwt?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    }
}