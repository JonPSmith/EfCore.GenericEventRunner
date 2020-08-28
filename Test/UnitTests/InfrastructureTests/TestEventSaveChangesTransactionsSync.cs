// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DataLayer;
using EntityClasses;
using EntityClasses.DomainEvents;
using EntityClasses.SupportClasses;
using GenericEventRunner.DomainParts;
using GenericEventRunner.ForDbContext;
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

        //Tests before and after
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

        //---------------------------------------------------------------
        //Test of exceptions/status with error

        [Fact]
        public void TestExceptionDuringPostSave()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>(logs);
            {
                //ATTEMPT
                var tax = context.TaxRates.First();
                tax.AddEvent(new EventTestDuringExceptionHandler(), EventToSend.DuringSave);
                var ex = Assert.Throws<ApplicationException>(() => context.SaveChanges());

                //VERIFY
                ex.Message.ShouldEqual(nameof(DuringHandlerThrowsException));
            }
        }

        [Fact]
        public void TestExceptionDuringPreSave()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>(logs);
            {
                //ATTEMPT
                var tax = context.TaxRates.First();
                throw new NotImplementedException();
                tax.AddEvent(new EventTestDuringExceptionHandler(), EventToSend.DuringSave);
                var ex = Assert.Throws<ApplicationException>(() => context.SaveChanges());

                //VERIFY
                ex.Message.ShouldEqual(nameof(DuringHandlerThrowsException));
            }
        }

        [Fact]
        public void TestBadStatusDuringPostSave()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>(logs);
            {
                //ATTEMPT
                var tax = context.TaxRates.First();
                throw new NotImplementedException();
                tax.AddEvent(new EventTestDuringReturnError(), EventToSend.DuringSave);
                var ex = Assert.Throws<GenericEventRunnerStatusException>(() => context.SaveChanges());

                //VERIFY
                context.StatusFromLastSaveChanges.IsValid.ShouldBeFalse();
            }
        }

        [Fact]
        public void TestBadStatusDuringPreSave()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>(logs);
            {
                //ATTEMPT
                var tax = context.TaxRates.First();
                tax.AddEvent(new EventTestDuringReturnError(), EventToSend.DuringSave);
                var ex = Assert.Throws<GenericEventRunnerStatusException>(() => context.SaveChanges());

                //VERIFY
                context.StatusFromLastSaveChanges.IsValid.ShouldBeFalse();
            }
        }

        //--------------------------------------------------------------
        //add own transaction

        [Fact]
        public void TestAddBookCausesDuringEventLogsInTransactionCommitOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>(logs);
            {
                using (var transaction = context.Database.BeginTransaction())
                {

                    //ATTEMPT
                    var book = Book.CreateBookWithEvent("test");
                    context.Add(book);
                    context.SaveChanges();

                    transaction.Commit();
                }

                //VERIFY
                context.Books.Count().ShouldEqual(1);
                logs.Count.ShouldEqual(2);
                logs[0].Message.ShouldEqual("D1: About to run a DuringSave event handler Infrastructure.DuringEventHandlers.NewBookDuringEventHandler.");
                logs[1].Message.ShouldStartWith("Log from NewBookDuringEventHandler. Unique value = ");
            }
        }

        [Fact]
        public void TestAddBookCausesDuringEventLogsInTransactionRollbackOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var logs = new List<LogOutput>();
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>(logs);
            {
                using (var transaction = context.Database.BeginTransaction())
                {

                    //ATTEMPT
                    var book = Book.CreateBookWithEvent("test");
                    context.Add(book);
                    context.SaveChanges();
                }//rollback on dispose

                //VERIFY
                context.Books.Count().ShouldEqual(0);
                logs.Count.ShouldEqual(2);
                logs[0].Message.ShouldEqual("D1: About to run a DuringSave event handler Infrastructure.DuringEventHandlers.NewBookDuringEventHandler.");
                logs[1].Message.ShouldStartWith("Log from NewBookDuringEventHandler. Unique value = ");
            }
        }

        [Fact]
        public void TestAddBookCausesDuringEventLogsInTransactionWithRetryCommitOk()
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
                context.SaveChanges();

                //VERIFY
                context.Books.Count().ShouldEqual(1);
                logs.Count.ShouldEqual(2);
                logs[0].Message.ShouldEqual("D1: About to run a DuringSave event handler Infrastructure.DuringEventHandlers.NewBookDuringEventHandler.");
                logs[1].Message.ShouldStartWith("Log from NewBookDuringEventHandler. Unique value = ");
            }
        }

        //------------------------------------------------------------------
        //test action after DetectChanged

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