using System;
using System.Threading.Tasks;
using AIRelief.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AIRelief.Data
{
    /// <summary>
    /// Ensures a default System Admin account exists.
    /// Runs on every startup — skips if the account is already present.
    /// </summary>
    public static class SeedAdminUser
    {
        private const string AdminEmail = "alex.matthewman@gmail.com";
        private const string AdminName = "Alex Matthewman";
        private const string DefaultPassword = "TempPass123!";

        public static async Task Run(AIReliefContext db, UserManager<IdentityUser> userManager)
        {
            // Check if the app-level User record already exists
            if (await db.Users.AnyAsync(u => u.Email == AdminEmail))
                return;

            // 1. Create the Identity account (or find existing)
            var identityUser = await userManager.FindByEmailAsync(AdminEmail);
            if (identityUser == null)
            {
                identityUser = new IdentityUser
                {
                    UserName = AdminEmail,
                    Email = AdminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(identityUser, DefaultPassword);
                if (!result.Succeeded)
                    throw new Exception($"Failed to create admin Identity user: {string.Join(", ", result.Errors)}");
            }

            // 2. Create the app-level User record with SystemAdmin auth level
            db.Users.Add(new User
            {
                Email = AdminEmail,
                Name = AdminName,
                AuthLevel = AuthLevel.SystemAdmin,
                TenantCode = "relief",
                CreatedDate = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }
    }
}
