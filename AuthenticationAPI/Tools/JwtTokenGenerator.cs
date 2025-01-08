using AuthenticationAPI.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthenticationAPI.Tools
{
    public class JwtTokenGenerator
    {
        private readonly JwtTokenDefaults _jwtTokenDefaults;

        public JwtTokenGenerator(IOptions<JwtTokenDefaults> jwtTokenDefaults)
        {
            _jwtTokenDefaults = jwtTokenDefaults.Value;
        }

        public JwtTokenResponse GenerateToken(JwtTokenRequest request)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, request.UserID),
                new Claim(ClaimTypes.Name, request.Username),
                new Claim(ClaimTypes.Email, request.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in request.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtTokenDefaults.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_jwtTokenDefaults.AccessTokenExpires);

            var token = new JwtSecurityToken(
                issuer: _jwtTokenDefaults.ValidIssuer,
                audience: _jwtTokenDefaults.ValidAudience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: credentials
            );

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            return new JwtTokenResponse(tokenHandler.WriteToken(token), expires);
        }

        public string GenerateRefreshToken()
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
            return Convert.ToBase64String(key.Key);
        }
    }
}
