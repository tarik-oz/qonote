using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qonote.Core.Domain.Identity;

namespace Qonote.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Surname)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.RefreshToken)
            .HasMaxLength(256);
    }
}