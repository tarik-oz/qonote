using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Qonote.Core.Application.Abstractions.Data;
using Qonote.Infrastructure.Persistence.Context;
using Qonote.Infrastructure.Persistence.Repositories;

namespace Qonote.Infrastructure.Persistence;

public static class ServiceRegistration
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("QonoteDbConnection")));

        services.AddScoped(typeof(IReadRepository<,>), typeof(ReadRepository<,>));
        services.AddScoped(typeof(IWriteRepository<,>), typeof(WriteRepository<,>));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}