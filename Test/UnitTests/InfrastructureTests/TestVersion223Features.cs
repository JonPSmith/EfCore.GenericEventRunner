// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer;
using EntityClasses;
using EntityClasses.DomainEvents;
using GenericEventRunner.ForHandlers.Internal;
using Infrastructure.BeforeEventHandlers;
using Test.EfHelpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.InfrastructureTests
{
    public class TestVersion223Features
    {
        private readonly ITestOutputHelper _output;

        public TestVersion223Features(ITestOutputHelper output)
        {
            _output = output;
        }


        [Fact]
        public void TestEntityAndEventComparer()
        {
            var tax = new TaxRate(DateTime.Now, 123);
            var e1 = new EntityAndEvent(tax, new DeDupEvent(() => { }));
            var e2 = new EntityAndEvent(tax, new DeDupEvent(() => { }));
            var e3 = new EntityAndEvent(tax, new NewBookEvent());
            var e4 = new EntityAndEvent(tax, new NewBookEvent());

            e1.Equals(e2).ShouldBeTrue();
            e1.Equals(e3).ShouldBeFalse();

            var ees = (new List<EntityAndEvent> { e1, e2, e3, e4 }).Distinct().ToList();
            ees.Count.ShouldEqual(3);
        }

        [Fact]
        public void TestDeDupEvents()
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
    }
}