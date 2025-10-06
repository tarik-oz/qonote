using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Core.Application.Abstractions.Queries;
using Qonote.Infrastructure.Persistence.Context;
using Qonote.Infrastructure.Persistence.Queries;
using Qonote.Infrastructure.Persistence.Repositories;
using Qonote.Infrastructure.Persistence.Operations.ChangeTracking;
using Qonote.Infrastructure.Persistence.Operations.AccountDeletion;
using Qonote.Infrastructure.Persistence.Operations.SectionUiState;
using Qonote.Core.Application.Abstractions.Security;

namespace Qonote.Infrastructure.Persistence;

public static class ServiceRegistration
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("QonoteDbConnection")));

        services.AddScoped(typeof(IReadRepository<,>), typeof(ReadRepository<,>));
        services.AddScoped(typeof(IWriteRepository<,>), typeof(WriteRepository<,>));
        services.AddScoped<INoteQueries, NoteQueries>();

        services.AddScoped<ISidebarImpactEvaluator, SidebarImpactEvaluator>();
        services.AddScoped<IAccountDeletionService, AccountDeletionService>();
        services.AddScoped<ISectionUiStateStore, SectionUiStateStore>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}