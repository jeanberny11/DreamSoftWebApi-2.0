using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using DreamSoft.Application.Behaviors;

namespace DreamSoft.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register pipeline behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        // Register AutoMapper — scans this assembly for all Profile subclasses
        // (including MappingProfile which auto-discovers IMapFrom<T> implementations)
        var assembly = Assembly.GetExecutingAssembly();
        services.AddAutoMapper(cfg => cfg.AddMaps(assembly));

        return services;
    }
}