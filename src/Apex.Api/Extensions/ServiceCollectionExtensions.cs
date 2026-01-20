using System.Reflection;
using Apex.Application.Abstractions.Messaging;

namespace Apex.Api.Extensions;

/// <summary>
/// Extension methods for IServiceCollection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all command and query handlers from the Application assembly using reflection.
    /// </summary>
    public static IServiceCollection AddApplicationHandlers(this IServiceCollection services)
    {
        var applicationAssembly = Assembly.Load("Apex.Application");

        // Register all ICommandHandler implementations
        RegisterHandlers(
            services,
            applicationAssembly,
            typeof(ICommandHandler<>),
            ServiceLifetime.Scoped);

        RegisterHandlers(
            services,
            applicationAssembly,
            typeof(ICommandHandler<,>),
            ServiceLifetime.Scoped);

        // Register all IQueryHandler implementations
        RegisterHandlers(
            services,
            applicationAssembly,
            typeof(IQueryHandler<,>),
            ServiceLifetime.Scoped);

        return services;
    }

    private static void RegisterHandlers(
        IServiceCollection services,
        Assembly assembly,
        Type handlerInterface,
        ServiceLifetime lifetime)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == handlerInterface))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            services.Add(new ServiceDescriptor(handlerType, handlerType, lifetime));
        }
    }
}
