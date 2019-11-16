// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Reflection;
using DataLayer;
using GenericEventRunner.ForDbContext;
using GenericEventRunner.ForHandlers;
using Infrastructure.BeforeEventHandlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Test.EfHelpers
{
    public static class DbAndDiExtensions
    {
        public static ExampleDbContext CreateAndSeedDbWithDiForHandlers(this DbContextOptions<ExampleDbContext> options)
        {
            var context = CreateDbWithDiForHandlers(options);

            context.Database.EnsureCreated();
            context.SeedTaxAndStock();

            return context;
        }

        public static ExampleDbContext CreateDbWithDiForHandlers(this DbContextOptions<ExampleDbContext> options)
        {
            var services = new ServiceCollection();
            services.RegisterEventRunner();
            services.RegisterEventHandlers(Assembly.GetAssembly(typeof(OrderCreatedHandler)));
            services.AddScoped(x =>
                new ExampleDbContext(options, x.GetRequiredService<IEventsRunner>()));
            var serviceProvider = services.BuildServiceProvider();
            var context = serviceProvider.GetRequiredService<ExampleDbContext>();
            return context;
        }
    }
}