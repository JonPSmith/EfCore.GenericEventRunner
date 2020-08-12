// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using EntityClasses;
using GenericEventRunner.DomainParts;
using GenericEventRunner.ForHandlers;
using Test.EfHelpers;
using Test.EventsAndHandlers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.InfrastructureTests
{
    public class TestAsyncEventHandlers
    {
        [Fact]
        public void TestCreateDbWithDiForHandlersOneEvent()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateDbWithDiForHandlers(logs, typeof(BeforeHandlerDoNothing));
            {
                context.Database.EnsureCreated();
                var entity = context.SeedTwoTaxRates().First();

                //ATTEMPT
                entity.AddEvent(new EventDoNothing());
                context.SaveChanges();

                //VERIFY
                logs.Count.ShouldEqual(1);
                logs[0].Message.ShouldEqual("B1: About to run a BeforeSave event handler Test.EventsAndHandlers.BeforeHandlerDoNothing.");
            }
        }

        [Fact]
        public void TestCreateDbWithDiForHandlersTwoEvents()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateDbWithDiForHandlers(logs, typeof(BeforeHandlerDoNothing), typeof(AfterHandlerDoNothing));
            {
                context.Database.EnsureCreated();
                var entity = context.SeedTwoTaxRates().First();

                //ATTEMPT
                entity.AddEvent(new EventDoNothing(), EventToSend.BeforeAndAfterSave);
                context.SaveChanges();

                //VERIFY
                logs.Count.ShouldEqual(2);
                logs[0].Message.ShouldEqual("B1: About to run a BeforeSave event handler Test.EventsAndHandlers.BeforeHandlerDoNothing.");
                logs[1].Message.ShouldEqual("A1: About to run a AfterSave event handler Test.EventsAndHandlers.AfterHandlerDoNothing.");
            }
        }

        [Fact]
        public async Task TestCreateDbWithDiForHandlersBeforeHandlerDoNothingAsyncOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateDbWithDiForHandlers(logs, typeof(BeforeHandlerDoNothingAsync));
            {
                context.Database.EnsureCreated();
                var entity = context.SeedTwoTaxRates().First();

                //ATTEMPT
                entity.AddEvent(new EventDoNothing());
                await context.SaveChangesAsync();

                //VERIFY
                logs.Count.ShouldEqual(1);
                logs[0].Message.ShouldEqual("B1: About to run a BeforeSave event handler Test.EventsAndHandlers.BeforeHandlerDoNothingAsync.");
            }
        }

        [Fact]
        public async Task TestCreateDbWithDiForHandlersBeforeHandlerDoNothingSyncAndAsyncOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateDbWithDiForHandlers(logs, typeof(BeforeHandlerDoNothing), typeof(BeforeHandlerDoNothingAsync));
            {
                context.Database.EnsureCreated();
                var entity = context.SeedTwoTaxRates().First();

                //ATTEMPT
                entity.AddEvent(new EventDoNothing());
                await context.SaveChangesAsync();

                //VERIFY
                logs.Count.ShouldEqual(1);
                logs[0].Message.ShouldEqual("B1: About to run a BeforeSave event handler Test.EventsAndHandlers.BeforeHandlerDoNothingAsync.");
            }
        }

        [Fact]
        public void TestCreateDbWithDiForHandlersBeforeHandlerDoNothingAsyncBad()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateDbWithDiForHandlers(logs, typeof(BeforeHandlerDoNothingAsync));
            {
                context.Database.EnsureCreated();
                var entity = context.SeedTwoTaxRates().First();

                //ATTEMPT
                entity.AddEvent(new EventDoNothing());
                var ex = Assert.Throws<GenericEventRunnerException>(() => context.SaveChanges());

                //VERIFY
                ex.Message.ShouldEqual("Could not find a BeforeSave event handler for the event EventDoNothing. Their was a suitable async event handler available, but you didn't call SaveChangesAsync.");
            }
        }

        [Fact]
        public async Task TestCreateDbWithDiForHandlersAfterHandlerDoNothingAsyncOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateDbWithDiForHandlers(logs, typeof(AfterHandlerDoNothingAsync));
            {
                context.Database.EnsureCreated();
                var entity = context.SeedTwoTaxRates().First();

                //ATTEMPT
                entity.AddEvent(new EventDoNothing(), EventToSend.AfterSave);
                await context.SaveChangesAsync();

                //VERIFY
                logs.Count.ShouldEqual(1);
                logs[0].Message.ShouldEqual("A1: About to run a AfterSave event handler Test.EventsAndHandlers.AfterHandlerDoNothingAsync.");
            }
        }

        [Fact]
        public async Task TestCreateDbWithDiForHandlersAfterHandlerDoNothingSyncAndAsyncOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateDbWithDiForHandlers(logs, typeof(AfterHandlerDoNothing), typeof(AfterHandlerDoNothingAsync));
            {
                context.Database.EnsureCreated();
                var entity = context.SeedTwoTaxRates().First();

                //ATTEMPT
                entity.AddEvent(new EventDoNothing(), EventToSend.AfterSave);
                await context.SaveChangesAsync();

                //VERIFY
                logs.Count.ShouldEqual(1);
                logs[0].Message.ShouldEqual("A1: About to run a AfterSave event handler Test.EventsAndHandlers.AfterHandlerDoNothingAsync.");
            }
        }

        [Fact]
        public void TestCreateDbWithDiForHandlersAfterHandlerDoNothingAsyncBad()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateDbWithDiForHandlers(logs, typeof(AfterHandlerDoNothingAsync));
            {
                context.Database.EnsureCreated();
                var entity = context.SeedTwoTaxRates().First();

                //ATTEMPT
                entity.AddEvent(new EventDoNothing(), EventToSend.AfterSave);
                var ex = Assert.Throws<GenericEventRunnerException>(() => context.SaveChanges());

                //VERIFY
                ex.Message.ShouldEqual("Could not find a AfterSave event handler for the event EventDoNothing. Their was a suitable async event handler available, but you didn't call SaveChangesAsync.");
            }
        }
    }

}