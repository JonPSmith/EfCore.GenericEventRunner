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
using GenericEventRunner.DomainParts;
using GenericEventRunner.ForDbContext;
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
    public class TestEventSaveChangesTransactionsAsync
    {
        [Fact]
        public void TestAddBookAddsDuringEvent()
        {
            //SETUP

            //ATTEMPT
            var book = Book.CreateBookWithEvent("test");

            //VERIFY
            book.GetDuringSaveEvents().Count.ShouldEqual(1);
        }

        [Fact]
        public async Task TestAddBookCausesDuringEventLogsAsync()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>(logs);
            {
                //ATTEMPT
                var book = Book.CreateBookWithEvent("test");
                context.Add(book);
                await context.SaveChangesAsync();

                //VERIFY
                logs.Count.ShouldEqual(2);
                logs[0].Message.ShouldEqual("D1: About to run a DuringSave event handler Infrastructure.DuringEventHandlers.NewBookDuringEventHandlerAsync.");
                logs[1].Message.ShouldStartWith("Log from NewBookDuringEventHandlerAsync. Unique value = ");
            }
        }

        [Fact]
        public async Task TestAddBookCausesDuringEventLogsWithBeforeSaveAsync()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>(logs);
            {
                //ATTEMPT
                var book = Book.CreateBookWithEvent("test");
                book.AddEvent(new NewBookEventButBeforeSave(), EventToSend.DuringSave);
                context.Add(book);
                await context.SaveChangesAsync();

                //VERIFY
                logs.Count.ShouldEqual(4);
                logs[0].Message.ShouldEqual("D1: About to run a DuringBeforeSave event handler Infrastructure.DuringEventHandlers.NewBookDuringButBeforeSaveEventHandlerAsync.");
                logs[1].Message.ShouldStartWith("Log from NewBookDuringButBeforeSaveEventHandlerAsync. Unique value = ");
                logs[2].Message.ShouldEqual("D1: About to run a DuringSave event handler Infrastructure.DuringEventHandlers.NewBookDuringEventHandlerAsync.");
                logs[3].Message.ShouldStartWith("Log from NewBookDuringEventHandlerAsync. Unique value =");
            }
        }
    }
}