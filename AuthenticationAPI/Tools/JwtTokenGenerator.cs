using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthenticationAPI.Models;
using System.Security.Cryptography;

namespace AuthenticationAPI.Tools
{
    public class JwtTokenGenerator
    {
        public static JwtTokenResponse GenerateToken(GetCheckAppUserModel checkAppUserModel)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, checkAppUserModel.Id),
                new Claim(ClaimTypes.Name, checkAppUserModel.Username),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtTokenDefaults.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(JwtTokenDefaults.AccessTokenExpiration);

            var token = new JwtSecurityToken(
                issuer: JwtTokenDefaults.ValidIssuer,
                audience: JwtTokenDefaults.ValidAudience,
                claims: claims,
                notBefore: DateTime.Now,
                expires: expires,
                signingCredentials: credentials
            );

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            return new JwtTokenResponse(tokenHandler.WriteToken(token), expires);
        }

        public static string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
