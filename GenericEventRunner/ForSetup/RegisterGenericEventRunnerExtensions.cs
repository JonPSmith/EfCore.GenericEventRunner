// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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
        public static void RegisterGenericEventRunner(this IServiceCollection services,
            params Assembly[] assembliesToScan)
        {
            if (!assembliesToScan.Any())
                assembliesToScan = new Assembly[] { Assembly.GetCallingAssembly() };

            services.RegisterGenericEventRunner(new GenericEventRunnerConfig(), assembliesToScan);
        }

        /// <summary>
        /// This register the Generic EventRunner and the various event handlers you have created in the assemblies you provide.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config">A GenericEventRunnerConfig instance with your own settings.</param>
        /// <param name="assembliesToScan">Series of assemblies to scan. If not provided then scans the calling assembly</param>
        public static void RegisterGenericEventRunner(this IServiceCollection services,
            IGenericEventRunnerConfig config,
            params Assembly[] assembliesToScan)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            if (!assembliesToScan.Any())
                assembliesToScan = new Assembly[]{ Assembly.GetCallingAssembly()};

            var eventHandlersToRegister = new List<(Type classType, Type interfaceType)>();
            var someAfterSaveHandlersFound = false;
            foreach (var assembly in assembliesToScan)
            {
                eventHandlersToRegister.AddRange(ClassesWithGivenEventHandlerType(typeof(IBeforeSaveEventHandler<>), assembly));
                var count = eventHandlersToRegister.Count;
                if (!config.NotUsingAfterSaveHandlers)
                    eventHandlersToRegister
                        .AddRange(ClassesWithGivenEventHandlerType(typeof(IAfterSaveEventHandler<>), assembly));
                
                someAfterSaveHandlersFound |= (eventHandlersToRegister.Count > count);
            }

            if (!someAfterSaveHandlersFound)
                config.NotUsingAfterSaveHandlers = true;

            foreach (var (implementationType, interfaceType) in eventHandlersToRegister)
            {
                var attr = implementationType.GetCustomAttribute<EventHandlerConfigAttribute>();
                var lifeTime = attr?.HandlerLifetime ?? ServiceLifetime.Transient;
                if (lifeTime == ServiceLifetime.Transient)
                    services.AddTransient(interfaceType, implementationType);
                else if (lifeTime == ServiceLifetime.Scoped)
                    services.AddScoped(interfaceType, implementationType);
                else
                    services.AddSingleton(interfaceType, implementationType);
            }

            services.AddSingleton<IGenericEventRunnerConfig>(config);
            services.AddScoped<IEventsRunner, EventsRunner>();
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