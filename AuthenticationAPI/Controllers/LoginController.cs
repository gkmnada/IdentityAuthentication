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

        public LoginController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
                                GetCheckAppUserModel checkAppUserModel = new GetCheckAppUserModel();
                                checkAppUserModel.Id = user.Id;
                                checkAppUserModel.Username = user.UserName;
                                checkAppUserModel.IsExist = true;
                                var token = JwtTokenGenerator.GenerateToken(checkAppUserModel);
                                return Created("", token);
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
