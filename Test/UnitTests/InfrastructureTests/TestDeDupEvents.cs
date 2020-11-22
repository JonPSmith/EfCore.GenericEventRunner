// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer;
using EntityClasses;
using EntityClasses.DomainEvents;
using GenericEventRunner.DomainParts;
using GenericEventRunner.ForHandlers.Internal;
using Infrastructure.BeforeEventHandlers;
using Test.EfHelpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.InfrastructureTests
{
    public class TestDeDupEvents
    {
        private readonly ITestOutputHelper _output;

        public TestDeDupEvents(ITestOutputHelper output)
        {
            _output = output;
        }


        [Fact]
        public void TestEntityAndEventComparer()
        {
            var tax1 = new TaxRate(DateTime.Now, 123);
            var tax2 = new TaxRate(DateTime.Now, 123);
            var d1 = new EntityAndEvent(tax1, new DeDupEvent(() => { }));
            var d2 = new EntityAndEvent(tax1, new DeDupEvent(() => { }));

            var d3 = new EntityAndEvent(tax2, new DeDupEvent(() => { }));

            var n1 = new EntityAndEvent(tax1, new NewBookEvent());
            var n2 = new EntityAndEvent(tax1, new NewBookEvent());

            d1.Equals(d2).ShouldBeTrue();
            d1.Equals(d3).ShouldBeFalse();


            d1.Equals(n1).ShouldBeFalse();
            n1.Equals(n2).ShouldBeFalse();

            var ees = (new List<EntityAndEvent> { d1,d2,d3,n1,n2 }).Distinct().ToList();
            ees.Select(x => x.EntityEvent.GetType().Name).ShouldEqual(new []{ "DeDupEvent", "DeDupEvent", "NewBookEvent", "NewBookEvent" });
            ees[0].ShouldEqual(d1);
            ees[1].ShouldEqual(d3);
        }

        [Fact]
        public void TestEntityAndEventComparerDistinctOrder()
        {
            var tax1 = new TaxRate(DateTime.Now, 123);
            var tax2 = new TaxRate(DateTime.Now, 123);
            var d1 = new EntityAndEvent(tax1, new DeDupEvent(() => { }));
            var d2 = new EntityAndEvent(tax1, new DeDupEvent(() => { }));

            var d3 = new EntityAndEvent(tax2, new DeDupEvent(() => { }));
            var d4 = new EntityAndEvent(tax2, new DeDupEvent(() => { }));

            var ees = (new List<EntityAndEvent> { d1, d2, d3, d2, d4}).Distinct().ToList();
            ees.Count.ShouldEqual(2);
            ReferenceEquals(ees[0], d1).ShouldBeTrue();
            ReferenceEquals(ees[0], d1).ShouldBeTrue();
            ees[0].ShouldEqual(d1);
            ees[1].ShouldEqual(d3);
        }

        [Fact]
        public void TestDeDupBeforeEvents()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>(logs);
            {
                var tax = new TaxRate(DateTime.Now, 123);
                context.Add(tax);
                context.SaveChanges();
                logs.Clear();

                //ATTEMPT
                var normalCount = 0;
                var deDepCount = 0;
                tax.AddEvent(new TaxRateChangedEvent(123, () => normalCount++));
                tax.AddEvent(new TaxRateChangedEvent(123, () => normalCount++));
                tax.AddEvent(new DeDupEvent(() => deDepCount++));
                tax.AddEvent(new DeDupEvent(() => deDepCount++));

                context.SaveChanges();

                //VERIFY
                normalCount.ShouldEqual(2);
                deDepCount.ShouldEqual(1);
                foreach (var logOutput in logs)
                {
                    _output.WriteLine(logOutput.Message);
                }
            }
        }

        [Fact]
        public void TestDeDupBeforeDuringAfterEvents()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>(logs);
            {
                var tax = new TaxRate(DateTime.Now, 123);
                context.Add(tax);
                context.SaveChanges();
                logs.Clear();

                //ATTEMPT
                var deDepCount = 0;
                tax.AddEvent(new DeDupEvent(() => deDepCount++), EventToSend.BeforeAndAfterSave);
                tax.AddEvent(new DeDupEvent(() => deDepCount++), EventToSend.BeforeAndAfterSave);
                tax.AddEvent(new DeDupEvent(() => deDepCount++), EventToSend.DuringSave);
                tax.AddEvent(new DeDupEvent(() => deDepCount++), EventToSend.DuringSave);

                context.SaveChanges();

                //VERIFY
                deDepCount.ShouldEqual(3);
                foreach (var logOutput in logs)
                {
                    _output.WriteLine(logOutput.Message);
                }
            }
        }
    }
}