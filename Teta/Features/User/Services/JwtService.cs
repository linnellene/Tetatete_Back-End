using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TetaBackend.Features.User.Interfaces;

namespace TetaBackend.Features.User.Services;

public class JwtService: IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Guid userId, string email)
    {
        var claims = new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, userId.ToString()),
            new (ClaimTypes.Email, email)
        };

        var secret = _configuration.GetSection("Jwt:Secret").Value;

        if (secret is null)
        {
            return string.Empty;
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public JwtSecurityToken? ValidateAndDecodeJwtToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        
        var secret = _configuration.GetSection("Jwt:Secret").Value;

        if (secret is null)
        {
            return null;
        }
        
        var encodedSecret = Encoding.ASCII.GetBytes(secret); 

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(encodedSecret),
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidateIssuer = false,
                ValidateAudience = false,
            }, out var validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            
            return jwtToken;
        }
        catch
        {
            return null;
        }
    }
}