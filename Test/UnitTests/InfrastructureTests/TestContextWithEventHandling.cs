// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer;
using EntityClasses;
using EntityClasses.DomainEvents;
using EntityClasses.SupportClasses;
using Test.EfHelpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.InfrastructureTests
{
    public class TestContextWithEventHandling
    {
        [Fact]
        public void TestCreateOrderCheckEventsProduced()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var context = options.CreateAndSeedDbWithDiForHandlers();
            {
                var itemDto = new BasketItemDto
                {
                    ProductCode = context.ProductStocks.First().ProductCode,
                    NumOrdered = 2,
                    ProductPrice = 123
                };

                //ATTEMPT
                var order = new Order("test", DateTime.Now, new List<BasketItemDto> {itemDto});

                //VERIFY
                order.TotalPriceNoTax.ShouldEqual(2*123);
                order.GetBeforeSaveEventsThenClear().Select(x => x.GetType())
                    .ShouldEqual(new []{typeof(OrderCreatedEvent), typeof(AllocateProductEvent)});
            }
        }

        [Fact]
        public void TestCreateOrderCheckTaxAndGrandTotalSet()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var context = options.CreateAndSeedDbWithDiForHandlers();
            {
                var itemDto = new BasketItemDto
                {
                    ProductCode = context.ProductStocks.First().ProductCode,
                    NumOrdered = 2,
                    ProductPrice = 123
                };

                //ATTEMPT
                var order = new Order("test", DateTime.Now, new List<BasketItemDto> { itemDto });
                context.Add(order);
                context.SaveChanges();

                //VERIFY
                order.TotalPriceNoTax.ShouldEqual(2 * 123);
                order.TaxRatePercent.ShouldEqual(9);
                order.GrandTotalPrice.ShouldEqual(order.TotalPriceNoTax * (1 + order.TaxRatePercent / 100));
            }
        }

    }
}