using Microsoft.AspNetCore.Identity;
using Qonote.Core.Domain.Identity;

namespace Qonote.Core.Application.Extensions;

public static class UserManagerExtensions
{
    public static async Task<(string, DateTime)> GenerateAndSetEmailConfirmationCodeAsync(this UserManager<ApplicationUser> userManager, ApplicationUser user, int expiryMinutes = 3)
    {
        var confirmationCode = new Random().Next(100000, 999999).ToString();
        var expiryTime = DateTime.UtcNow.AddMinutes(expiryMinutes);

        user.EmailConfirmationCode = confirmationCode;
        user.EmailConfirmationCodeExpiry = expiryTime;

        await userManager.UpdateAsync(user);

        return (confirmationCode, expiryTime);
    }
}
