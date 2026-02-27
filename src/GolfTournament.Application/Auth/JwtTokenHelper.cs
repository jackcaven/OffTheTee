using GolfTournament.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GolfTournament.Application.Auth;

internal static class JwtTokenHelper
{
    internal static AuthResultDto GenerateToken(ApplicationUser user, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured.");
        var issuer = jwtSettings["Issuer"] ?? "OffTheTee";
        var audience = jwtSettings["Audience"] ?? "OffTheTee";
        var expiryMinutes = int.TryParse(jwtSettings["ExpiryMinutes"], out var minutes) ? minutes : 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("displayName", user.DisplayName)
        };

        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new AuthResultDto(
            AccessToken: new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt: expiresAt,
            UserId: user.Id,
            Email: user.Email!,
            DisplayName: user.DisplayName);
    }
}
