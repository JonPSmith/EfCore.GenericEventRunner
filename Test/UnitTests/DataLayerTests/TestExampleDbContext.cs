// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using DataLayer;
using EntityClasses;
using GenericEventRunner.ForEntities;
using Test.EfHelpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayerTests
{
    public class TestExampleDbContextNoEvents
    {
        [Fact]
        public void TestCreateDatabaseAndSeedProductStock()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            using (var context = new ExampleDbContext(options))
            {

                //ATTEMPT
                context.Database.EnsureCreated();
                context.SeedExampleProductStock();

                //VERIFY
                context.ProductStocks.Count().ShouldEqual(3);
            }
        }

        [Fact]
        public void TestIdentity()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            using (var context = new ExampleDbContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedExampleProductStock();

                //ATTEMPT
                var entity = context.ChangeTracker.Entries<EntityEvents>().First().Entity;

                //VERIFY

                
            }
        }

    }
}