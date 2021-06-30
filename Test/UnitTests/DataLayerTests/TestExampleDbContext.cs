// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using EntityClasses;
using Test.EfHelpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayerTests
{
    public class TestExampleDbContextNoEvents
    {
        [Fact]
        public void TestCreateDatabaseSeedTaxAndStock()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            using (var context = new ExampleDbContext(options))
            {

                //ATTEMPT
                context.Database.EnsureCreated();
                context.SeedTaxAndStock();

                //VERIFY
                context.TaxRates.Count().ShouldEqual(2);
                context.ProductStocks.Count().ShouldEqual(3);
            }
        }

        [Fact]
        public void TestCheckSaveChangesReturningCount()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            using (var context = new ExampleDbContext(options))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                context.Add(new TaxRate(DateTime.UtcNow, 4));
                var numUpdates = context.SaveChanges();

                //VERIFY
                numUpdates.ShouldEqual(1);
            }
        }

        [Fact]
        public async Task TestCheckSaveChangesAsyncReturningCount()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            using (var context = new ExampleDbContext(options))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                context.Add(new TaxRate(DateTime.UtcNow, 4));
                var numUpdates = await context.SaveChangesAsync();

                //VERIFY
                numUpdates.ShouldEqual(1);
            }
        }

    }
}