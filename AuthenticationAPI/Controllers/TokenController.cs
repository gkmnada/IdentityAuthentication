using AuthenticationAPI.Context;
using AuthenticationAPI.Entities;
using AuthenticationAPI.Models;
using AuthenticationAPI.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public TokenController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> RefreshToken(string refreshToken)
        {
            var existingToken = _context.RefreshTokens.FirstOrDefault(x => x.Token == refreshToken);

            if (existingToken == null || existingToken.IsRevoked)
            {
                return BadRequest("Invalid refresh token");
            }

            var user = await _userManager.FindByIdAsync(existingToken.UserId);

            if (user == null)
            {
                return BadRequest("User not found");
            }

            var checkAppUserModel = new GetCheckAppUserModel
            {
                Id = user.Id,
                Username = user.UserName,
                IsExist = true
            };

            var token = JwtTokenGenerator.GenerateToken(checkAppUserModel);

            existingToken.Token = JwtTokenGenerator.GenerateRefreshToken();
            existingToken.Expiration = DateTime.Now.AddDays(JwtTokenDefaults.RefreshTokenExpiration);
            existingToken.IsRevoked = false;

            _context.RefreshTokens.Update(existingToken);
            await _context.SaveChangesAsync();

            return Ok(new { token.Token, token.ExpireDate, RefreshToken = existingToken.Token });
        }
    }
}
