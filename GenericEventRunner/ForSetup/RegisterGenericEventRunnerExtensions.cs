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
            var someDuringSaveHandlersFound = false;
            var someAfterSaveHandlersFound = false;
            foreach (var assembly in assembliesToScan)
            {
                eventHandlersToRegister.AddRange(ClassesWithGivenEventHandlerType(typeof(IBeforeSaveEventHandler<>), assembly));
                eventHandlersToRegister.AddRange(ClassesWithGivenEventHandlerType(typeof(IBeforeSaveEventHandlerAsync<>), assembly));

                var count = eventHandlersToRegister.Count;
                if (!config.NotUsingDuringSaveHandlers)
                {
                    eventHandlersToRegister
                        .AddRange(ClassesWithGivenEventHandlerType(typeof(IDuringSaveEventHandler<>), assembly));
                    eventHandlersToRegister
                        .AddRange(ClassesWithGivenEventHandlerType(typeof(IDuringSaveEventHandlerAsync<>), assembly));
                }
                someDuringSaveHandlersFound |= (eventHandlersToRegister.Count > count);

                count = eventHandlersToRegister.Count;
                if (!config.NotUsingAfterSaveHandlers)
                {
                    eventHandlersToRegister
                        .AddRange(ClassesWithGivenEventHandlerType(typeof(IAfterSaveEventHandler<>), assembly));
                    eventHandlersToRegister
                        .AddRange(ClassesWithGivenEventHandlerType(typeof(IAfterSaveEventHandlerAsync<>), assembly));
                }
                someAfterSaveHandlersFound |= (eventHandlersToRegister.Count > count);
            }

            if (!someDuringSaveHandlersFound)
                config.NotUsingDuringSaveHandlers = true;
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

            if (services.Contains(new ServiceDescriptor(typeof(IEventsRunner), typeof(EventsRunner), ServiceLifetime.Scoped),
                new ServiceDescriptorCompare()))
                throw new InvalidOperationException("You can only call this method once to register the GenericEventRunner and event handlers.");
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