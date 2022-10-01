using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using RTGSWebApi.Authentication;
using RTGSWebApi.Data;
using System;
using System.Linq;

namespace RTGSWebApi.Data
{
    public class SeedDatabase
    {
        //Test purpose only
        public static void Initialize(IServiceProvider serviceProvider)
        {
            try
            {
                var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                context.Database.EnsureCreated();

                if (!context.Users.Any())
                {
                    ApplicationUser user = new ApplicationUser()
                    {
                        UserName = "TestApiUser",
                        Email = "abc@gmail.com"
                    };
                    userManager.CreateAsync(user, "Test@1234");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
