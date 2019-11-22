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
    public static class RegisterEventHandlingExtensions
    {
        public static void RegisterEventHandlers(this IServiceCollection services, params Assembly[] assembliesToScan)
        {
            if (!assembliesToScan.Any())
                assembliesToScan = new Assembly[]{ Assembly.GetExecutingAssembly()};

            var eventHandlersToRegister = new List<(Type classType, Type interfaceType)>();
            foreach (var assembly in assembliesToScan)
            {
                eventHandlersToRegister.AddRange(ClassesWithGivenEventHandlerType(typeof(IBeforeSaveEventHandler<>), assembly));
                eventHandlersToRegister
                    .AddRange(ClassesWithGivenEventHandlerType(typeof(IAfterSaveEventHandler<>), assembly));
            }

            foreach (var classAndInterface in eventHandlersToRegister)
            {
                services.AddTransient(classAndInterface.interfaceType, classAndInterface.classType);
            }
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

        public static void RegisterEventRunner(this IServiceCollection services, IGenericEventRunnerConfig config = null)
        {
            services.AddSingleton<IGenericEventRunnerConfig>(config ?? new GenericEventRunnerConfig());
            services.AddScoped<IEventsRunner, EventsRunner>();
        }
    }
}