using Argo.Application.Common.Mapping;
using Argo.Application.Common.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Argo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddArgoAppliction(this IServiceCollection services)
    {
        var currentAssembly = typeof(DependencyInjection).Assembly;

        services.AddScoped<IDispatcher, Dispatcher>();
        services.AddSingleton<IMapper>(_ => Mapper.Build(mapper => RegisterAllMapModules(mapper, currentAssembly)));

        services.RegisterRequestHandlers(currentAssembly);

        return services;
    }

    private static void RegisterAllMapModules(Mapper mapper, Assembly assembly)
    {
        var moduleTypes = assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && !type.IsInterface && typeof(IMapModule).IsAssignableFrom(type));

        foreach (var moduleType in moduleTypes)
        {
            var moduleInstance = (IMapModule)Activator.CreateInstance(moduleType)!;
            moduleInstance.RegisterMaps(mapper);
        }
    }

    private static IServiceCollection RegisterRequestHandlers(this IServiceCollection services, Assembly assembly)
    {
        var moduleTypes = assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && !type.IsInterface);

        foreach (var moduleType in moduleTypes)
        {
            var serviceInterfaces = moduleType.GetInterfaces()
                .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

            foreach (var serviceType in serviceInterfaces)
            {
                if (services.Any(sd => sd.ServiceType == moduleType))
                    throw new InvalidOperationException($"Request handler {moduleType.FullName} is already registered.");

                services.AddScoped(serviceType, moduleType);
            }

        }

        return services;
    }
}
