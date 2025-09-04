using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Qonote.Domain.Entities;
using Qonote.Domain.Identity;

namespace Qonote.Infrastructure.Persistence.Context;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Custom Domain Entities
    public DbSet<NoteGroup> NoteGroups { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<Block> Blocks { get; set; }
    public DbSet<SectionUIState> SectionUIStates { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
