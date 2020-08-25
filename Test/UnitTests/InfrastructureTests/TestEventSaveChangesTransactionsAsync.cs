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
        public async Task TestAddBookAddsDuringEvent()
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

        [Fact]
        public async Task TestAddBookCausesDuringEventLogsInTransactionCommitOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>(logs);
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    //ATTEMPT
                    var book = Book.CreateBookWithEvent("test");
                    context.Add(book);
                    await context.SaveChangesAsync();

                    await transaction.CommitAsync();
                }

                //VERIFY
                context.Books.Count().ShouldEqual(1);
                logs.Count.ShouldEqual(2);
                logs[0].Message.ShouldEqual("D1: About to run a DuringSave event handler Infrastructure.DuringEventHandlers.NewBookDuringEventHandlerAsync.");
                logs[1].Message.ShouldStartWith("Log from NewBookDuringEventHandlerAsync. Unique value = ");
            }
        }

        [Fact]
        public async Task TestAddBookCausesDuringEventLogsInTransactionRollbackOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>(logs);
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    //ATTEMPT
                    var book = Book.CreateBookWithEvent("test");
                    context.Add(book);
                    await context.SaveChangesAsync();
                }//rollback on dispose

                //VERIFY
                context.Books.Count().ShouldEqual(0);
                logs.Count.ShouldEqual(2);
                logs[0].Message.ShouldEqual("D1: About to run a DuringSave event handler Infrastructure.DuringEventHandlers.NewBookDuringEventHandlerAsync.");
                logs[1].Message.ShouldStartWith("Log from NewBookDuringEventHandlerAsync. Unique value = ");
            }
        }

        [Fact]
        public async Task TestAddBookCausesDuringEventLogsInTransactionWithRetryCommitOk()
        {
            //SETUP
            var options = this.CreateOptionsWithRetryExecutions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateDbWithDiForHandlers<ExampleDbContext, OrderCreatedHandler>(logs);
            {
                context.Database.EnsureCreated();
                context.WipeAllDataFromDatabase();

                //ATTEMPT
                var book = Book.CreateBookWithEvent("test");
                context.Add(book);
                await context.SaveChangesAsync();

                //VERIFY
                context.Books.Count().ShouldEqual(1);
                logs.Count.ShouldEqual(2);
                logs[0].Message.ShouldEqual("D1: About to run a DuringSave event handler Infrastructure.DuringEventHandlers.NewBookDuringEventHandlerAsync.");
                logs[1].Message.ShouldStartWith("Log from NewBookDuringEventHandlerAsync. Unique value = ");
            }
        }

        [Fact]
        public async Task TestAddBookNoActionToUpdateDates()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>();
            {
                //ATTEMPT
                var book = Book.CreateBookWithEvent("test");
                context.Add(book);
                await context.SaveChangesAsync();

                //VERIFY
                book.WhenCreatedUtc.ShouldEqual(new DateTime());
            }
        }

        [Fact]
        public async Task TestAddBookAddedActionToUpdateDates()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var config = new GenericEventRunnerConfig();
            config.AddActionToRunAfterDetectChanges<ExampleDbContext>(localContext =>
            {
                foreach (var entity in localContext.ChangeTracker.Entries()
                    .Where(e =>
                        e.State == EntityState.Added ||
                        e.State == EntityState.Modified))
                {
                    var tracked = entity.Entity as ICreatedUpdated;
                    tracked?.LogChange(entity.State == EntityState.Added, entity);
                }
            });
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>(null, config);
            {
                //ATTEMPT
                var book = Book.CreateBookWithEvent("test");
                context.Add(book);
                await context.SaveChangesAsync();
                book.WhenCreatedUtc.Subtract(DateTime.UtcNow).TotalMilliseconds.ShouldBeInRange(-100, 10);

                Thread.Sleep(1000);
                book.ChangeTitle("new title");
                await context.SaveChangesAsync();

                //VERIFY
                book.LastUpdatedUtc.Subtract(DateTime.UtcNow).TotalMilliseconds.ShouldBeInRange(-100, 10);
            }
        }

    }
}