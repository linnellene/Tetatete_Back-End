using System.Security.Claims;
using TetaBackend.Features.User.Interfaces;

namespace TetaBackend.Features.Shared.Middlewares;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IJwtService jwtService)
    {
        var authorizationHeader = context.Request.Headers.Authorization;

        if (!authorizationHeader.ToString().StartsWith("Bearer"))
        {
            await _next(context);

            return;
        }

        var token = authorizationHeader.ToString().Substring("Bearer ".Length).Trim();
        
        var jwt = jwtService.ValidateAndDecodeJwtToken(token);

        var userId = jwt?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (userId is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid user id in auth token.");
            await _next(context);

            return;
        }
        
        context.Items["UserId"] = userId;
        
        await _next(context);
    }
}