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
    public class LoginController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public LoginController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, true);

                if (result.Succeeded)
                {
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        return BadRequest("Email is not confirmed");
                    }
                    else
                    {
                        var login = await _signInManager.PasswordSignInAsync(model.Username, model.Password, true, true);

                        if (login.Succeeded)
                        {
                            if (await _userManager.IsInRoleAsync(user, "User"))
                            {
                                GetCheckAppUserModel checkAppUserModel = new GetCheckAppUserModel
                                {
                                    Id = user.Id,
                                    Username = user.UserName,
                                    IsExist = true
                                };

                                var token = JwtTokenGenerator.GenerateToken(checkAppUserModel);
                                var refreshToken = JwtTokenGenerator.GenerateRefreshToken();

                                var entity = new RefreshToken
                                {
                                    Token = refreshToken,
                                    UserId = user.Id,
                                    Expiration = DateTime.Now.AddDays(JwtTokenDefaults.RefreshTokenExpiration),
                                    IsRevoked = false
                                };

                                await _context.RefreshTokens.AddAsync(entity);
                                await _context.SaveChangesAsync();

                                return Created("", new { token.Token, token.ExpireDate, RefreshToken = refreshToken });
                            }
                            else
                            {
                                return BadRequest("User is not in role");
                            }
                        }
                        else if (login.IsLockedOut)
                        {
                            return BadRequest("User is locked out");
                        }
                        else
                        {
                            return BadRequest("Invalid login attempt");
                        }
                    }
                }
                else if (result.IsLockedOut)
                {
                    return BadRequest("User is locked out");
                }
                else
                {
                    return BadRequest("Invalid login attempt");
                }
            }
            else
            {
                return BadRequest("User not found");
            }
        }
    }
}
