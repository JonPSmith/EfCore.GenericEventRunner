// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using DataLayer;
using EntityClasses;
using GenericEventRunner.ForSetup;
using Microsoft.EntityFrameworkCore;
using TestSupport.EfHelpers;

namespace Test.EfHelpers
{
    public static class SeedExtensions
    {

        public static ExampleDbContext CreateAndSeedDbWithDiForHandlers<TRunner>(this DbContextOptions<ExampleDbContext> options,
            List<LogOutput> logs = null, IGenericEventRunnerConfig config = null) where TRunner : class
        {
            var context = options.CreateDbWithDiForHandlers<ExampleDbContext, TRunner>(logs, config);

            context.Database.EnsureCreated();
            context.SeedTaxAndStock();

            return context;
        }

        public static List<ProductStock> SeedTaxAndStock(this ExampleDbContext context)
        {
            context.SeedTwoTaxRates();
            return context.SeedExampleProductStock();
        }

        public static List<ProductStock> SeedExampleProductStock(this ExampleDbContext context)
        {
            var prodStocks = new List<ProductStock>
            {
                new ProductStock("Product1", 5),
                new ProductStock("Product2", 10),
                new ProductStock("Product3", 20),
            };
            context.AddRange(prodStocks);
            context.SaveChanges();
            return prodStocks;
        }

        public static List<TaxRate> SeedTwoTaxRates(this ExampleDbContext context)
        {
            var rateNow = new TaxRate(DateTime.Today, 4);
            var rate2Days = new TaxRate(DateTime.Today.AddDays(2), 9);
            context.AddRange(rateNow, rate2Days);
            context.SaveChanges();
            return new List<TaxRate>{ rateNow, rate2Days};
        }
    }
}