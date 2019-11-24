// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using EntityClasses;
using EntityClasses.DomainEvents;
using EntityClasses.SupportClasses;
using GenericEventRunner.ForEntities;
using GenericEventRunner.ForHandlers;
using Test.EfHelpers;
using Test.EventsAndHandlers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.InfrastructureTests
{
    public class TestEventSaveChangesAsync
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
                    ProductName = context.ProductStocks.OrderBy(x => x.NumInStock).First().ProductName,
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
        public async Task TestOrderCreatedHandler()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var context = options.CreateAndSeedDbWithDiForHandlers();
            {
                var itemDto = new BasketItemDto
                {
                    ProductName = context.ProductStocks.OrderBy(x => x.NumInStock).First().ProductName,
                    NumOrdered = 2,
                    ProductPrice = 123
                };

                //ATTEMPT
                var order = new Order("test", DateTime.Now, new List<BasketItemDto> { itemDto });
                context.Add(order);
                await context.SaveChangesAsync();

                //VERIFY
                order.TotalPriceNoTax.ShouldEqual(2 * 123);
                order.TaxRatePercent.ShouldEqual(4);
                order.GrandTotalPrice.ShouldEqual(order.TotalPriceNoTax * (1 + order.TaxRatePercent / 100));
                context.ProductStocks.OrderBy(x => x.NumInStock).First().NumAllocated.ShouldEqual(2);
            }
        }

        [Fact]
        public async Task TestOrderCreatedHandlerExceptionNotEnoughStock()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var context = options.CreateAndSeedDbWithDiForHandlers();
            {
                var itemDto = new BasketItemDto
                {
                    ProductName = context.ProductStocks.OrderBy(x => x.NumInStock).First().ProductName,
                    NumOrdered = 10,
                    ProductPrice = 123
                };

                //ATTEMPT
                var order = new Order("test", DateTime.Now, new List<BasketItemDto> { itemDto });
                context.Add(order);
                var ex = await Assert.ThrowsAsync<GenericEventRunnerException>(async () => await context.SaveChangesAsync());

                //VERIFY
                ex.Message.ShouldEqual(@"Problem when writing to the database: Failed with 1 error
I could not accept this order because there wasn't enough Product1 in stock.");
            }
        }

        [Fact]
        public async Task TestOrderCreatedHandlerLogs()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateAndSeedDbWithDiForHandlers(logs);
            {
                var itemDto = new BasketItemDto
                {
                    ProductName = context.ProductStocks.OrderBy(x => x.NumInStock).First().ProductName,
                    NumOrdered = 2,
                    ProductPrice = 123
                };

                //ATTEMPT
                var order = new Order("test", DateTime.Now, new List<BasketItemDto> { itemDto });
                context.Add(order);
                await context.SaveChangesAsync();;

                //VERIFY
                logs.Count.ShouldEqual(3);
                logs[0].Message.ShouldEqual("B1: About to run a BeforeSave event handler Infrastructure.BeforeEventHandlers.OrderCreatedHandler.");
                logs[1].Message.ShouldEqual("B1: About to run a BeforeSave event handler Infrastructure.BeforeEventHandlers.AllocateProductHandler.");
                logs[2].Message.ShouldEqual("B2: About to run a BeforeSave event handler Infrastructure.BeforeEventHandlers.TaxRateChangedHandler.");
            }
        }

        [Fact]
        public async Task TestOrderDispatchedHandler()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var context = options.CreateAndSeedDbWithDiForHandlers();
            {
                var itemDto = new BasketItemDto
                {
                    ProductName = context.ProductStocks.OrderBy(x => x.NumInStock).First().ProductName,
                    NumOrdered = 2,
                    ProductPrice = 123
                };
                var order = new Order("test", DateTime.Now, new List<BasketItemDto> { itemDto });
                context.Add(order);
                await context.SaveChangesAsync();;

                //ATTEMPT
                order.OrderHasBeenDispatched(DateTime.Now.AddDays(10));
                await context.SaveChangesAsync();;

                //VERIFY
                order.TotalPriceNoTax.ShouldEqual(2 * 123);
                order.TaxRatePercent.ShouldEqual(9);
                order.GrandTotalPrice.ShouldEqual(order.TotalPriceNoTax * (1 + order.TaxRatePercent / 100));
                context.ProductStocks.OrderBy(x => x.NumInStock).First().NumAllocated.ShouldEqual(0);
                context.ProductStocks.OrderBy(x => x.NumInStock).First().NumInStock.ShouldEqual(3);
            }
        }

        [Fact]
        public async Task TestOrderDispatchedHandlerLogs()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateAndSeedDbWithDiForHandlers(logs);
            {
                var itemDto = new BasketItemDto
                {
                    ProductName = context.ProductStocks.OrderBy(x => x.NumInStock).First().ProductName,
                    NumOrdered = 2,
                    ProductPrice = 123
                };
                var order = new Order("test", DateTime.Now, new List<BasketItemDto> { itemDto });
                context.Add(order);
                await context.SaveChangesAsync();;
                logs.Clear();

                //ATTEMPT
                order.OrderHasBeenDispatched(DateTime.Now.AddDays(10));
                await context.SaveChangesAsync();;

                //VERIFY
                logs.Count.ShouldEqual(3);
                logs[0].Message.ShouldEqual("B1: About to run a BeforeSave event handler Infrastructure.BeforeEventHandlers.OrderDispatchedBeforeHandler.");
                logs[1].Message.ShouldEqual("B2: About to run a BeforeSave event handler Infrastructure.BeforeEventHandlers.TaxRateChangedHandler.");
                logs[2].Message.ShouldEqual("A1: About to run a AfterSave event handler Infrastructure.AfterEventHandlers.OrderDispatchedAfterHandler.");
            }
        }

        [Theory]
        [InlineData(EventToSend.Before)]
        [InlineData(EventToSend.After)]
        public async Task TestMissingHandlerThrowsException(EventToSend beforeAfter)
        {

            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var context = options.CreateAndSeedDbWithDiForHandlers();
            {
                var tax = new TaxRate(DateTime.Now, 6);
                context.Add(tax);

                //ATTEMPT
                tax.AddEvent(new EventWithNoHandler(), beforeAfter);
                var ex = await Assert.ThrowsAsync<GenericEventRunnerException>(async () => await context.SaveChangesAsync());

                //VERIFY
                ex.Message.ShouldEqual(beforeAfter == EventToSend.Before
                    ? $"Could not find a BeforeSave event handler for the event {typeof(EventWithNoHandler).Name}."
                    : $"Could not find a AfterSave event handler for the event {typeof(EventWithNoHandler).Name}.");
            }
        }

        [Fact]
        public async Task TestBeforeHandlerThrowsException()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var context = options.CreateAndSeedDbWithDiForHandlers();
            {
                var tax = new TaxRate(DateTime.Now, 6);
                context.Add(tax);

                //ATTEMPT
                tax.AddEvent(new EventTestBeforeExceptionHandler());
                var ex = await Assert.ThrowsAsync<ApplicationException>(async () => await context.SaveChangesAsync());

                //VERIFY
                ex.Message.ShouldEqual(nameof(BeforeHandlerThrowsException));
            }
        }

        [Fact]
        public async Task TestBeforeHandlerThrowsExceptionWithAttribute()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var context = options.CreateAndSeedDbWithDiForHandlers();
            {
                var tax = new TaxRate(DateTime.Now, 6);
                context.Add(tax);

                //ATTEMPT
                tax.AddEvent(new EventTestExceptionHandlerWithAttribute());
                var ex = await Assert.ThrowsAsync<ApplicationException>(async () => await context.SaveChangesAsync());

                //VERIFY
                ex.Message.ShouldEqual(nameof(BeforeHandlerThrowsExceptionWithAttribute));
            }
        }

        [Fact]
        public async Task TestCircularEventException()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var context = options.CreateAndSeedDbWithDiForHandlers();
            {
                var tax = new TaxRate(DateTime.Now, 6);
                context.Add(tax);

                //ATTEMPT
                tax.AddEvent(new EventCircularEvent());
                var ex = await Assert.ThrowsAsync<GenericEventRunnerException>(async () => await context.SaveChangesAsync());

                //VERIFY
                ex.Message.ShouldEqual("The BeforeSave event loop exceeded the config's MaxTimesToLookForBeforeEvents value of 6. This implies a circular sets of events.");
            }
        }

    }
}