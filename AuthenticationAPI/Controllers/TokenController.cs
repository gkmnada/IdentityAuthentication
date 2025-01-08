using AuthenticationAPI.Context;
using AuthenticationAPI.Entities;
using AuthenticationAPI.Models;
using AuthenticationAPI.Tools;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AuthenticationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly JwtTokenDefaults _jwtTokenDefaults;

        public TokenController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, JwtTokenGenerator jwtTokenGenerator, IOptions<JwtTokenDefaults> jwtTokenDefaults)
        {
            _userManager = userManager;
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
            _jwtTokenDefaults = jwtTokenDefaults.Value;
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            var existingToken = _context.RefreshTokens.FirstOrDefault(x => x.Token == refreshTokenDto.RefreshToken);

            if (existingToken == null || existingToken.IsRevoked)
            {
                return BadRequest("Invalid refresh token");
            }

            if (existingToken.Expiration < DateTime.UtcNow)
            {
                existingToken.IsRevoked = true;

                _context.RefreshTokens.Update(existingToken);
                await _context.SaveChangesAsync();

                return BadRequest("Refresh token expired");
            }

            var user = await _userManager.FindByIdAsync(existingToken.UserID);

            if (user == null)
            {
                return BadRequest("User not found");
            }

            var token = _jwtTokenGenerator.GenerateToken(new JwtTokenRequest
            {
                UserID = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Roles = (await _userManager.GetRolesAsync(user)).ToList()
            });

            existingToken.Token = _jwtTokenGenerator.GenerateRefreshToken();

            _context.RefreshTokens.Update(existingToken);
            await _context.SaveChangesAsync();

            return Ok(new { AccessToken = token.Token, ExpireDate = token.ExpireDate, RefreshToken = existingToken.Token });
        }

        #region DTO
        public class RefreshTokenDto
        {
            public string RefreshToken { get; set; }
        }
        #endregion
    }
}
