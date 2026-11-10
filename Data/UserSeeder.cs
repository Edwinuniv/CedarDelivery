using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = serviceProvider.GetRequiredService<ILogger<UserSeeder>>();

            await CreateUserWithRole(userManager, logger,
                "admin@cedardelivery.com",
                config["Seed:AdminPassword"],
                "Admin",
                "User",
                Roles.Admin);

            await CreateUserWithRole(userManager, logger,
                "merchant@cedardelivery.com",
                config["Seed:MerchantPassword"],
                "Merchant",
                "User",
                Roles.Merchant);

            var driver = await CreateUserWithRole(userManager, logger,
                "driver@cedardelivery.com",
                config["Seed:DriverPassword"],
                "Driver",
                "User",
                Roles.Driver);

            await CreateUserWithRole(userManager, logger,
                "customer@cedardelivery.com",
                config["Seed:CustomerPassword"],
                "Customer",
                "User",
                Roles.Customer);

            // A driver can't work without a driver profile; real drivers get one when they apply.
            if (driver != null && !await context.DriverProfiles.AnyAsync(d => d.UserId == driver.Id))
            {
                context.DriverProfiles.Add(new DriverProfile
                {
                    UserId = driver.Id,
                    VehicleType = VehicleType.Motorcycle,
                    PlateNumber = "DEMO-1",
                    IsApproved = true,
                    IsOnline = false,
                    CreatedAt = DateTime.Now
                });
                await context.SaveChangesAsync();
            }
        }

        // Returns the user (existing or newly created), or null if it was skipped or failed.
        private static async Task<User?> CreateUserWithRole(UserManager<User> userManager, ILogger logger, string email, string? password, string firstName, string lastName, string role)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                return existing;
            }

            if (string.IsNullOrEmpty(password))
            {
                logger.LogWarning("No seed password configured for {Email}; skipping.", email);
                return null;
            }

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

            if (!result.Succeeded)
            {
                // Usually a password that breaks the rules in Program.cs (length, digit, upper/lower case).
                logger.LogWarning("Could not seed {Email}: {Errors}", email,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                return null;
            }

            await userManager.AddToRoleAsync(user, role);
            return user;
        }
    }
}
