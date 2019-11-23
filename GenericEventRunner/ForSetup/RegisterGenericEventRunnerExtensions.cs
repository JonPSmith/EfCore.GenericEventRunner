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
    public static class RegisterGenericEventRunnerExtensions
    {
        public static void RegisterGenericEventRunner(this IServiceCollection services,
            params Assembly[] assembliesToScan)
        {
            services.RegisterGenericEventRunner(new GenericEventRunnerConfig(), assembliesToScan);
        }

        public static void RegisterGenericEventRunner(this IServiceCollection services,
            IGenericEventRunnerConfig config,
            params Assembly[] assembliesToScan)
        {
            if (!assembliesToScan.Any())
                assembliesToScan = new Assembly[]{ Assembly.GetExecutingAssembly()};

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

            foreach (var classAndInterface in eventHandlersToRegister)
            {
                var attr = classAndInterface.classType.GetCustomAttribute<EventHandlerConfigAttribute>();
                var lifeTime = attr?.HandlerLifetime ?? ServiceLifetime.Transient;
                if (lifeTime == ServiceLifetime.Transient)
                    services.AddTransient(classAndInterface.interfaceType, classAndInterface.classType);
                else if (lifeTime == ServiceLifetime.Scoped)
                    services.AddScoped(classAndInterface.interfaceType, classAndInterface.classType);
                else
                    services.AddSingleton(classAndInterface.interfaceType, classAndInterface.classType);
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