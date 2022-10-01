using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RTGSWebApi.Data;
using RTGSWebApi.Model;

namespace RTGSWebApi.Controllers
{
    public class LoginController : Controller
    {
        private UserManager<ApplicationUser> _userManager;

        //public AuthController(UserManager<UserDb> userManager, IOptions<ConfigurationOptions> options)
        //{
        //    this._userManager = userManager;
        //    Options = options;
        //}

        //[Route("Login")]
        //[HttpPost]
        //public async Task<IActionResult> Index([FromBody] LoginModel model)
        //{
        //    var user = await _userManager.FindByNameAsync(model.UserName);
        //    if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
        //    {
        //        var claims = new[]
        //        {
        //            new Claim(JwtRegisteredClaimNames.Sub,user.UserName),
        //            new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
        //        };
        //        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MySuperSecureKey"));

        //        var token = new JwtSecurityToken(
        //            issuer: "http://oec.com",
        //            audience: "http://oec.com",
        //            expires: DateTime.UtcNow.AddDays(Options.Value.TokenExpiration),
        //            claims: claims,
        //            signingCredentials: new Microsoft.IdentityModel.Tokens.SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        //        return Ok(new
        //        {
        //            token = new JwtSecurityTokenHandler().WriteToken(token),
        //            expiration = token.ValidTo
        //        });

        //    }

        //    return Unauthorized();
        //}
    }
}
