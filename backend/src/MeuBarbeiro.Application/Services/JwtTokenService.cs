using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MeuBarbeiro.Application.Abstractions.Services;
using MeuBarbeiro.Domain.Entities;
using MeuBarbeiro.Domain.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace MeuBarbeiro.Application.Services;

public class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public string GenerateToken(User user)
    {
        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();
        
        if (string.IsNullOrWhiteSpace(jwtSettings!.SecretKey))
        {
            throw new InvalidOperationException("A chave JWT não foi configurada.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings!.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}