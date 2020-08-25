// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Test.EfHelpers;
using Test.EventsAndHandlers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.InfrastructureTests
{
    public class TestEventSaveChangesTransactionsSync
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
        public void TestAddBookCausesDuringEventLogs()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>(logs);
            {
                //ATTEMPT
                var book = Book.CreateBookWithEvent("test");
                context.Add(book);
                context.SaveChanges();

                //VERIFY
                logs.Count.ShouldEqual(2);
                logs[0].Message.ShouldEqual("D1: About to run a DuringSave event handler Infrastructure.DuringEventHandlers.NewBookDuringEventHandler.");
                logs[1].Message.ShouldStartWith("Log from NewBookDuringEventHandler. Unique value = ");
            }
        }

        [Fact]
        public void TestAddBookCausesDuringEventLogsWithBeforeSave()
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
                context.SaveChanges();

                //VERIFY
                logs.Count.ShouldEqual(4);
                logs[0].Message.ShouldEqual("D1: About to run a DuringBeforeSave event handler Infrastructure.DuringEventHandlers.NewBookDuringButBeforeSaveEventHandler.");
                logs[1].Message.ShouldStartWith("Log from NewBookDuringButBeforeSaveEventHandler. Unique value = ");
                logs[2].Message.ShouldEqual("D1: About to run a DuringSave event handler Infrastructure.DuringEventHandlers.NewBookDuringEventHandler.");
                logs[3].Message.ShouldStartWith("Log from NewBookDuringEventHandler. Unique value =");
            }
        }

        [Fact]
        public void TestAddBookNoActionToUpdateDates()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>();
            {
                //ATTEMPT
                var book = Book.CreateBookWithEvent("test");
                context.Add(book);
                context.SaveChanges();

                //VERIFY
                book.WhenCreatedUtc.ShouldEqual(new DateTime());
            }
        }

        [Fact]
        public void TestAddBookAddedActionToUpdateDates()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var config = new GenericEventRunnerConfig();

            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>(null, config);
            {
                config.AddActionToRunAfterDetectChanges<ExampleDbContext>(() =>
                {
                    foreach (var entity in context.ChangeTracker.Entries()
                        .Where(e =>
                            e.State == EntityState.Added ||
                            e.State == EntityState.Modified))
                    {
                        var tracked = entity.Entity as ICreatedUpdated;
                        tracked?.LogChange(entity.State == EntityState.Added, entity);
                    }
                });
                //ATTEMPT
                var book = Book.CreateBookWithEvent("test");
                context.Add(book);
                context.SaveChanges();
                book.WhenCreatedUtc.Subtract(DateTime.UtcNow).TotalMilliseconds.ShouldBeInRange(-100,10);

                Thread.Sleep(1000);
                book.ChangeTitle("new title");
                context.SaveChanges();

                //VERIFY
                book.LastUpdatedUtc.Subtract(DateTime.UtcNow).TotalMilliseconds.ShouldBeInRange(-100, 10);
            }
        }
    }
}