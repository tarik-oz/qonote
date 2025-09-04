using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qonote.Domain.Identity;

namespace Qonote.Infrastructure.Persistence.Configurations;

public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        var roles = new List<ApplicationRole>
        {
            new() {Id = Guid.NewGuid().ToString(), Name = "Admin", NormalizedName = "ADMIN"},
            new() {Id = Guid.NewGuid().ToString(), Name = "FreeUser", NormalizedName = "FREEUSER"},
            new() {Id = Guid.NewGuid().ToString(), Name = "PremiumUser", NormalizedName = "PREMIUMUSER"}
        };

        builder.HasData(roles);
    }
}