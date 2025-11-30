using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiIdentityService.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace ApiIdentityService.Application.Security
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _config;

        public JwtTokenGenerator(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(
            User user,
            IEnumerable<string> roles,
            IEnumerable<string> permissions
        )
        {
            var issuer = _config["Jwt:Issuer"] ?? "rbac-issuer";
            var audience = _config["Jwt:Audience"] ?? "rbac-clients";
            var secret =
                _config["Jwt:SecretKey"] ?? throw new Exception("Jwt:SecretKey not configured");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new("email", user.Email),
            };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            // permisos como múltiples claims "permission"
            claims.AddRange(permissions.Select(p => new Claim("permission", p)));

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
