using System.Reflection;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Qonote.Core.Application.Abstractions.Factories;
using Qonote.Core.Application.Abstractions.Rules;
using Qonote.Core.Application.Behaviors;
using Qonote.Core.Application.Factories;

namespace Qonote.Core.Application;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register all IBusinessRule<TRequest> implementations via reflection
        var assembly = Assembly.GetExecutingAssembly();
        var ruleRegistrations = assembly.DefinedTypes
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.ImplementedInterfaces
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBusinessRule<>))
                .Select(i => new { Service = i, Implementation = t.AsType() }));

        foreach (var reg in ruleRegistrations)
        {
            services.AddTransient(reg.Service, reg.Implementation);
        }

        services.AddScoped<ILoginResponseFactory, LoginResponseFactory>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(BusinessRulesBehavior<,>));
        });

        return services;
    }
}
