// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using DataLayer;
using GenericEventRunner.ForDbContext;
using GenericEventRunner.ForHandlers;
using GenericEventRunner.ForSetup;
using Infrastructure.BeforeEventHandlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Test.EventsAndHandlers;
using TestSupport.EfHelpers;

namespace Test.EfHelpers
{
    public static class DbAndDiExtensions
    {
        public static ExampleDbContext CreateAndSeedDbWithDiForHandlers(this DbContextOptions<ExampleDbContext> options,
            List<LogOutput> logs = null, IGenericEventRunnerConfig config = null)
        {
            var context = options.CreateDbWithDiForHandlers(logs);

            context.Database.EnsureCreated();
            context.SeedTaxAndStock();

            return context;
        }

        public static ExampleDbContext CreateDbWithDiForHandlers(this DbContextOptions<ExampleDbContext> options,
            List<LogOutput> logs = null, IGenericEventRunnerConfig config = null)
        {
            var services = new ServiceCollection();
            if (logs != null)
            {
                services.AddSingleton<ILogger<EventsRunner>>(new Logger<EventsRunner>(new LoggerFactory(new[] { new MyLoggerProvider(logs) })));
            }
            else
            {
                services.AddSingleton<ILogger<EventsRunner>>(new NullLogger<EventsRunner>());
            }
            services.RegisterGenericEventRunner(config,
                Assembly.GetAssembly(typeof(OrderCreatedHandler)), 
                Assembly.GetAssembly(typeof(BeforeHandlerCircularEvent)));
            services.AddScoped(x =>
                new ExampleDbContext(options, x.GetRequiredService<IEventsRunner>()));
            var serviceProvider = services.BuildServiceProvider();
            var context = serviceProvider.GetRequiredService<ExampleDbContext>();
            return context;
        }
    }
}