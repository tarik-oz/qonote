using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Qonote.Core.Domain.Identity;

namespace Qonote.Infrastructure.Persistence.Seeding;

public static class IdentitySeeding
{
    public static async Task EnsureAdminRoleAndBootstrapAsync(this IServiceProvider services, IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Ensure Admin role exists
        const string adminRoleName = "Admin";
        if (!await roleManager.RoleExistsAsync(adminRoleName))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = adminRoleName, NormalizedName = adminRoleName.ToUpperInvariant() });
        }

        // Config-based bootstrap (optional)
        var enabledStr = configuration["AdminBootstrap:Enabled"]; // string to avoid missing extensions
        if (!bool.TryParse(enabledStr, out var enabled) || !enabled)
            return;

        // If an admin already exists, skip bootstrap
        var existingAdmins = await userManager.GetUsersInRoleAsync(adminRoleName);
        if (existingAdmins.Count > 0)
            return;

        var email = configuration["AdminBootstrap:Email"];
        var password = configuration["AdminBootstrap:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Name = "Admin",
                Surname = "User"
            };
            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                return; // best-effort
            }
        }

        if (!await userManager.IsInRoleAsync(user, adminRoleName))
        {
            await userManager.AddToRoleAsync(user, adminRoleName);
        }
    }
}
