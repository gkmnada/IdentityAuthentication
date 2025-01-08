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
    public class LoginController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly JwtTokenDefaults _jwtTokenDefaults;

        public LoginController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context, JwtTokenGenerator jwtTokenGenerator, IOptions<JwtTokenDefaults> jwtTokenDefaults)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
            _jwtTokenDefaults = jwtTokenDefaults.Value;
        }

        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user == null)
            {
                return BadRequest(new ApiErrorResponse { Code = "UserNotFound", Message = "User not found" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, true);

            if (!result.Succeeded)
            {
                return Unauthorized(new ApiErrorResponse { Code = "InvalidLoginAttempt", Message = "Invalid login attempt" });
            }
            else if (result.IsLockedOut)
            {
                return Unauthorized(new ApiErrorResponse { Code = "AccountLocked", Message = "Account is locked out" });
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return Unauthorized(new ApiErrorResponse { Code = "EmailNotConfirmed", Message = "Email is not confirmed" });
            }

            if (!await _userManager.IsInRoleAsync(user, "User"))
            {
                return Unauthorized(new ApiErrorResponse { Code = "UnauthorizedAccess", Message = "Unauthorized access" });
            }

            var token = _jwtTokenGenerator.GenerateToken(new JwtTokenRequest
            {
                UserID = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Roles = (await _userManager.GetRolesAsync(user)).ToList()
            });

            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

            await _context.RefreshTokens.AddAsync(new RefreshToken
            {
                Token = refreshToken,
                UserID = user.Id,
                Expiration = DateTime.UtcNow.AddDays(_jwtTokenDefaults.RefreshTokenExpires),
                IsRevoked = false
            });

            await _context.SaveChangesAsync();

            return Created("", new { AccessToken = token.Token, ExpireDate = token.ExpireDate, RefreshToken = refreshToken });
        }
    }
}
