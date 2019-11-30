// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer;
using EntityClasses;
using EntityClasses.DomainEvents;
using EntityClasses.SupportClasses;
using GenericEventRunner.ForEntities;
using GenericEventRunner.ForHandlers;
using GenericEventRunner.ForSetup;
using Infrastructure.BeforeEventHandlers;
using Test.EfHelpers;
using Test.EventsAndHandlers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.InfrastructureTests
{
    public class TestEventSaveChangesWithStatusSync
    {

        [Fact]
        public void TestOrderCreatedHandler()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>();
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
                var status = context.SaveChangesWithStatus();

                //VERIFY
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                order.TotalPriceNoTax.ShouldEqual(2 * 123);
                order.TaxRatePercent.ShouldEqual(4);
                order.GrandTotalPrice.ShouldEqual(order.TotalPriceNoTax * (1 + order.TaxRatePercent / 100));
                context.ProductStocks.OrderBy(x => x.NumInStock).First().NumAllocated.ShouldEqual(2);
            }
        }

        [Fact]
        public void TestOrderCreatedHandlerNotEnoughStockStatus()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>();
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
                var status = context.SaveChangesWithStatus();

                //VERIFY
                status.IsValid.ShouldBeFalse();
                status.GetAllErrors().ShouldEqual("I could not accept this order because there wasn't enough Product1 in stock.");
            }
        }

        [Fact]
        public void TestOrderCreatedHandlerNotEnoughStockStatusThenAgainToCheckOriginalEventsCleared()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>(logs);
            {
                var itemDto = new BasketItemDto
                {
                    ProductName = context.ProductStocks.OrderBy(x => x.NumInStock).First().ProductName,
                    NumOrdered = 10,
                    ProductPrice = 123
                };
                var order1 = new Order("test", DateTime.Now, new List<BasketItemDto> { itemDto });
                context.Add(order1);
                context.SaveChangesWithStatus().IsValid.ShouldBeFalse();

                //ATTEMPT
                logs.Clear();
                itemDto.NumOrdered = 2;
                var order2 = new Order("test", DateTime.Now, new List<BasketItemDto> { itemDto });
                context.Add(order2);
                var status = context.SaveChangesWithStatus();

                //VERIFY
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                logs.Count.ShouldEqual(3);
                logs[0].Message.ShouldEqual("B1: About to run a BeforeSave event handler Infrastructure.BeforeEventHandlers.OrderCreatedHandler.");
                logs[1].Message.ShouldEqual("B1: About to run a BeforeSave event handler Infrastructure.BeforeEventHandlers.AllocateProductHandler.");
                logs[2].Message.ShouldEqual("B2: About to run a BeforeSave event handler Infrastructure.BeforeEventHandlers.TaxRateChangedHandler.");
            }
        }

        [Fact]
        public void TestOrderDispatchedHandler()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>();
            {
                var itemDto = new BasketItemDto
                {
                    ProductName = context.ProductStocks.OrderBy(x => x.NumInStock).First().ProductName,
                    NumOrdered = 2,
                    ProductPrice = 123
                };
                var order = new Order("test", DateTime.Now, new List<BasketItemDto> { itemDto });
                context.Add(order);
                context.SaveChanges();

                //ATTEMPT
                order.OrderReadyForDispatch(DateTime.Now.AddDays(10));
                var status = context.SaveChangesWithStatus();

                //VERIFY
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                order.TotalPriceNoTax.ShouldEqual(2 * 123);
                order.TaxRatePercent.ShouldEqual(9);
                order.GrandTotalPrice.ShouldEqual(order.TotalPriceNoTax * (1 + order.TaxRatePercent / 100));
                context.ProductStocks.OrderBy(x => x.NumInStock).First().NumAllocated.ShouldEqual(0);
                context.ProductStocks.OrderBy(x => x.NumInStock).First().NumInStock.ShouldEqual(3);
            }
        }

        [Fact]
        public void TestOrderDispatchedHandlerLogs()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>(logs);
            {
                var itemDto = new BasketItemDto
                {
                    ProductName = context.ProductStocks.OrderBy(x => x.NumInStock).First().ProductName,
                    NumOrdered = 2,
                    ProductPrice = 123
                };
                var order = new Order("test", DateTime.Now, new List<BasketItemDto> { itemDto });
                context.Add(order);
                context.SaveChanges();
                logs.Clear();

                //ATTEMPT
                order.OrderReadyForDispatch(DateTime.Now.AddDays(10));
                var status = context.SaveChangesWithStatus();

                //VERIFY
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                logs.Count.ShouldEqual(3);
                logs[0].Message.ShouldEqual("B1: About to run a BeforeSave event handler Infrastructure.BeforeEventHandlers.OrderDispatchedBeforeHandler.");
                logs[1].Message.ShouldEqual("B2: About to run a BeforeSave event handler Infrastructure.BeforeEventHandlers.TaxRateChangedHandler.");
                logs[2].Message.ShouldEqual("A1: About to run a AfterSave event handler Infrastructure.AfterEventHandlers.OrderReadyToDispatchAfterHandler.");
            }
        }

        [Fact]
        public void TestBeforeHandlerThrowsException()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>();
            {
                var tax = new TaxRate(DateTime.Now, 6);
                context.Add(tax);

                //ATTEMPT
                tax.AddEvent(new EventTestBeforeExceptionHandler());
                var ex = Assert.Throws<ApplicationException>(() => context.SaveChangesWithStatus());

                //VERIFY
                ex.Message.ShouldEqual(nameof(BeforeHandlerThrowsException));
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TestStopOnFirstBeforeHandlerThatHasAnError(bool stopOnFirst)
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var config = new GenericEventRunnerConfig
            {
                StopOnFirstBeforeHandlerThatHasAnError = stopOnFirst
            };
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>(config: config);
            {
                var tax = new TaxRate(DateTime.Now, 6);
                context.Add(tax);

                //ATTEMPT
                tax.AddEvent(new EventTestBeforeReturnError());
                tax.AddEvent(new EventTestBeforeReturnError());
                var status = context.SaveChangesWithStatus();

                //VERIFY
                status.IsValid.ShouldBeFalse();
                status.Errors.Count.ShouldEqual(stopOnFirst ? 1 : 2);
                context.StatusFromLastSaveChanges.Errors.Count.ShouldEqual(stopOnFirst ? 1 : 2);
            }
        }

        [Fact]
        public void TestAfterHandlerThrowsException()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>();
            {
                var tax = new TaxRate(DateTime.Now, 6);
                context.Add(tax);

                //ATTEMPT
                tax.AddEvent(new EventTestAfterExceptionHandler(), EventToSend.AfterSave);
                var ex = Assert.Throws<ApplicationException>(() => context.SaveChangesWithStatus());

                //VERIFY
                ex.Message.ShouldEqual(nameof(AfterHandlerThrowsException));
            }
        }

    }
}