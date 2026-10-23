using Microsoft.AspNetCore.Identity;
using CedarDelivery.Constants;
using CedarDelivery.Models;

namespace CedarDelivery.Data
{
    // Creates one demo account per role on startup.
    // Passwords are read from configuration (user-secrets), never hard-coded:
    //   dotnet user-secrets set "Seed:AdminPassword" "..."
    // If a password is missing, that account is skipped.
    public class UserSeeder
    {
        public static async Task SeedUsersAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var config = serviceProvider.GetRequiredService<IConfiguration>();

            await CreateUserWithRole(userManager,
                "admin@cedardelivery.com",
                config["Seed:AdminPassword"],
                "Admin",
                "User",
                Roles.Admin);

            await CreateUserWithRole(userManager,
                "user@cedardelivery.com",
                config["Seed:UserPassword"],
                "Regular",
                "User",
                Roles.User);

            await CreateUserWithRole(userManager,
                "agent@cedardelivery.com",
                config["Seed:AgentPassword"],
                "Agent",
                "User",
                Roles.Agent);
        }

        private static async Task CreateUserWithRole(UserManager<User> userManager, string email, string? password, string firstName, string lastName, string role)
        {
            if (string.IsNullOrEmpty(password))
            {
                return;
            }

            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new User
                {
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}
