using JWTApi.Models;
using JWTApi.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JWTApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationUserController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationSetting _appSetting;

        public ApplicationUserController(UserManager<ApplicationUser>userManager, SignInManager<ApplicationUser>signInManager,IOptions<ApplicationSetting>appSetting)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _appSetting = appSetting.Value;
        }
        [HttpPost]
        [Route("Register")]
        //POST:api/ApplicationUser/Register
        public async Task<object> PostApplicationUser(vmApplicationUser model)
        {
            var applicationUser = new ApplicationUser() {
                FullName = model.FullName,
                UserName = model.UserName,
                Email = model.Email
            };
            try
            {
                var result =await _userManager.CreateAsync(applicationUser, model.Password);
                return Ok(result);

            }
            catch(Exception ex)
            {
                throw (ex);
            }


        }
        [HttpPost]
        [Route("Login")]
        //POST:api/ApplicationUser/Login
        public async Task<IActionResult> Login(vmLogin model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user !=null && await _userManager.CheckPasswordAsync(user,model.Password))
            {
                var tokenDiscriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[] {

                        new Claim("UserID", user.Id.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSetting.JWT_Secret)), SecurityAlgorithms.HmacSha256Signature)

                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDiscriptor);
                var token = tokenHandler.WriteToken(securityToken);
                return Ok(new { token });
            }
            else
            {
                return BadRequest(new { massage="UserName or Password is incorect"});
            }


          
        }

    }
}
