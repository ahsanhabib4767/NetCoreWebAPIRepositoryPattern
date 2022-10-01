using RTGSWebApi.Data;
using RTGSWebApi.Infrastructure.Utility;
using RTGSWebApi.Transaction;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RTGSWebApi.Data;
using RTGSWebApi.Model;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using RTGSWebApi.LoginSecurity;

namespace RTGSWebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
   
    [ApiController]
    public class AuthController : BaseController
    {
        private UserManager<ApplicationUser> _userManager;

        public AuthController(UserManager<ApplicationUser> userManager, IOptions<ConfigurationOptions> options,ILoginDP dP)
        {
            this._userManager = userManager;
            Options = options;
            LDp = dP;
        }

        [Route("Login")]
        [HttpPost]
        public async Task<IActionResult> Index([FromBody] LoginModel model)
        {
            //var user = await _userManager.FindByNameAsync(model.UserName);
            //if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            var checkCredential = await LDp.CheckCredential(model.UserName, model.Password);
            if(checkCredential == model.UserName){
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub,model.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                };
                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MySuperSecureKey"));

                var token = new JwtSecurityToken(
                    issuer: "http://oec.com",
                    audience: "http://oec.com",
                    expires: DateTime.UtcNow.AddDays(Options.Value.TokenExpiration),
                    claims: claims,
                    signingCredentials: new Microsoft.IdentityModel.Tokens.SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });

            }

            return Unauthorized();
        }

        [Route("CreateUser")]
        [HttpPost]
        public async Task<dynamic> CreateUser([FromBody] LoginModel model)
        {
            ApplicationUser user = new ApplicationUser()
            {
                UserName = model.UserName,
                Email = model.Email
            };
            await _userManager.CreateAsync(user, model.Password);

            return "User Created successfully";

        }

    }
}