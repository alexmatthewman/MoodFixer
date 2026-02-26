using Microsoft.AspNetCore.Identity;
using AIRelief.Models;

namespace AIRelief
{
    public static class SeedHelpers
    {
        public static async System.Threading.Tasks.Task EnsureInitialSystemUser(UserManager<IdentityUser> userManager, AIReliefContext context)
        {
            var email = "alex.matthewman@gmail.com";
            var password = "Ne14txxx!";

            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null)
                return; 

            var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return;

            // Create an application User record linked to IdentityUser
            var appUser = new AIRelief.Models.User
            {
                Name = "System Administrator",
                Email = email,
                IdentityUserId = user.Id,
                AuthLevel = AIRelief.Models.AuthLevel.SystemAdmin,
                CreatedDate = System.DateTime.UtcNow
            };
            context.Users.Add(appUser);
            await context.SaveChangesAsync();
        }
    }
}
