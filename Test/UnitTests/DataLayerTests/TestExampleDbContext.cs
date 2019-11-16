// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer;
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

    }
}