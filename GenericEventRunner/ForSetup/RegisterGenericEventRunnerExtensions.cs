// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using GenericEventRunner.ForDbContext;
using GenericEventRunner.ForHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace GenericEventRunner.ForSetup
{
    /// <summary>
    /// This contains the extensions methods to register the GenericEventRunner and the various event handlers you have created
    /// </summary>
    public static class RegisterGenericEventRunnerExtensions
    {
        /// <summary>
        /// This register the Generic EventRunner and the various event handlers you have created in the assemblies you provide.
        /// NOTE: Uses default GenericEventRunnerConfig settings.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembliesToScan">Series of assemblies to scan. If not provided then scans the calling assembly</param>
        public static List<string> RegisterGenericEventRunner(this IServiceCollection services,
            params Assembly[] assembliesToScan)
        {
            return services.RegisterGenericEventRunner(new GenericEventRunnerConfig(), assembliesToScan);
        }

        /// <summary>
        /// This register the Generic EventRunner and the various event handlers you have created in the assemblies you provide.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config">A GenericEventRunnerConfig instance with your own settings.</param>
        /// <param name="assembliesToScan">Series of assemblies to scan. If not provided then scans the calling assembly</param>
        /// <returns>List of what it registered - useful for debugging</returns>
        public static List<string> RegisterGenericEventRunner(this IServiceCollection services,
            IGenericEventRunnerConfig config,
            params Assembly[] assembliesToScan)
        {
            var debugLogs = new List<string>();

            if (config == null) throw new ArgumentNullException(nameof(config));

            if (!assembliesToScan.Any())
            {
                assembliesToScan = new Assembly[]{ Assembly.GetCallingAssembly()};
                debugLogs.Add("No assemblies provided so only scanning calling assembly.");
            }

            var someDuringSaveHandlersFound = false;
            var someAfterSaveHandlersFound = false;
            foreach (var assembly in assembliesToScan)
            {
                debugLogs.Add($"Starting scanning assembly {assembly.GetName().Name} for event handlers.");
                services.RegisterHandlersIfNotAlreadyRegistered(debugLogs,
                    typeof(IBeforeSaveEventHandler<>), assembly);
                services.RegisterHandlersIfNotAlreadyRegistered(debugLogs,
                    typeof(IBeforeSaveEventHandlerAsync<>), assembly);

                if (!config.NotUsingDuringSaveHandlers)
                {
                    someDuringSaveHandlersFound |= services.RegisterHandlersIfNotAlreadyRegistered(debugLogs,
                        typeof(IDuringSaveEventHandler<>), assembly);
                    someDuringSaveHandlersFound |= services.RegisterHandlersIfNotAlreadyRegistered(debugLogs,
                        typeof(IDuringSaveEventHandlerAsync<>), assembly);
                }

                if (!config.NotUsingAfterSaveHandlers)
                {
                    someAfterSaveHandlersFound |= services.RegisterHandlersIfNotAlreadyRegistered(debugLogs,
                    typeof(IAfterSaveEventHandler<>), assembly);
                    someAfterSaveHandlersFound |= services.RegisterHandlersIfNotAlreadyRegistered(debugLogs,
                        typeof(IAfterSaveEventHandlerAsync<>), assembly);
                }
            }

            if (!someDuringSaveHandlersFound)
            {
                debugLogs.Add(config.NotUsingDuringSaveHandlers
                    ? "You manually turned off During event handlers."
                    : "No During event handlers were found, so turned that part off.");

                config.NotUsingDuringSaveHandlers = true;
            }
            if (!someAfterSaveHandlersFound)
            {
                debugLogs.Add(config.NotUsingAfterSaveHandlers
                    ? "You manually turned off After event handlers."
                    : "No After event handlers were found, so turned that part off.");

                config.NotUsingAfterSaveHandlers = true;
            }

            if (services.Contains(new ServiceDescriptor(typeof(IEventsRunner), typeof(EventsRunner), ServiceLifetime.Transient),
                new ServiceDescriptorNoLifeTimeCompare()))
                throw new InvalidOperationException("You can only call this method once to register the GenericEventRunner and event handlers.");
            services.AddSingleton<IGenericEventRunnerConfig>(config);
            services.AddTransient<IEventsRunner, EventsRunner>();
            debugLogs.Add($"Finished by registering the {nameof(EventsRunner)} and {nameof(GenericEventRunnerConfig)}");

            return debugLogs;
        }

        private static bool  RegisterHandlersIfNotAlreadyRegistered(this IServiceCollection services,
            List<string> debugLogs,
            Type interfaceToLookFor, Assembly assembly)
        {
            var noLifeTimeCompare = new ServiceDescriptorNoLifeTimeCompare();
            var someFound = false;
            foreach (var (implementationType, interfaceType) in ClassesWithGivenEventHandlerType(interfaceToLookFor, assembly))
            {
                var attr = implementationType.GetCustomAttribute<EventHandlerConfigAttribute>();
                var lifeTime = attr?.HandlerLifetime ?? ServiceLifetime.Transient;

                var genericPart = interfaceType.GetGenericArguments();
                var indexCharToRemove = interfaceType.Name.IndexOf('`');
                var displayInterface = $"{interfaceType.Name.Substring(0, indexCharToRemove)}<{genericPart.Single().Name}>";

                if (services.Contains(new ServiceDescriptor(interfaceType, implementationType, lifeTime), noLifeTimeCompare))
                    debugLogs.Add($"Already registered {implementationType.Name} as {displayInterface}");
                else
                {
                    if (lifeTime == ServiceLifetime.Transient)
                        services.AddTransient(interfaceType, implementationType);
                    else if (lifeTime == ServiceLifetime.Scoped)
                        services.AddScoped(interfaceType, implementationType);
                    else
                        services.AddSingleton(interfaceType, implementationType);
                    debugLogs.Add($"Registered {implementationType.Name} as {displayInterface}. Lifetime: {lifeTime}");
                }
                someFound = true;
            }

            return someFound;
        }

        private static IEnumerable<(Type classType, Type interfaceType)> ClassesWithGivenEventHandlerType(Type interfaceToLookFor, Assembly assembly)
        {
            var allGenericClasses = assembly.GetExportedTypes()
                .Where(y => y.IsClass && !y.IsAbstract && !y.IsGenericType && !y.IsNested);
            var classesWithIHandle = from classType in allGenericClasses
                let interfaceType = classType.GetInterfaces()
                    .SingleOrDefault(y => y.IsGenericType && y.GetGenericTypeDefinition() == interfaceToLookFor)
                where interfaceType != null
                select (classType, interfaceType);
            return classesWithIHandle;
        }

    }
}