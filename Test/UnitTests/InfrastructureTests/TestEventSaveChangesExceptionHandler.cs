// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayer;
using EntityClasses;
using GenericEventRunner.ForSetup;
using Infrastructure.BeforeEventHandlers;
using Microsoft.EntityFrameworkCore;
using StatusGeneric;
using Test.EfHelpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.InfrastructureTests
{
    public class TestEventSaveChangesExceptionHandler
    {
        [Fact]
        public void TestUpdateProductStockOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>();
            {
                var stock = context.ProductStocks.OrderBy(x => x.NumInStock).First();
                
                //ATTEMPT
                stock.NumAllocated = 2;
                context.SaveChanges();

                //VERIFY
                var foundStock = context.Find<ProductStock>(stock.ProductName);
                foundStock.NumAllocated.ShouldEqual(2);
            }
        }

        [Fact]
        public void TestUpdateProductStockConcurrencyNoHandler()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>();
            {
                var stock = context.ProductStocks.OrderBy(x => x.NumInStock).First();

                //ATTEMPT
                stock.NumAllocated = 2;
                context.Database.ExecuteSqlRaw(
                    "UPDATE ProductStocks SET NumAllocated = @p0 WHERE ProductName = @p1", 3, stock.ProductName);
                var ex = Assert.Throws<DbUpdateConcurrencyException>(() => context.SaveChanges());

                //VERIFY
                ex.Message.ShouldStartWith("Database operation expected to affect 1 row(s) but actually affected 0 row(s).");
            }
        }

        [Fact]
        public void TestUpdateProductStockConcurrencyWithHandler()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExampleDbContext>();
            var config = new GenericEventRunnerConfig
            {
                SaveChangesExceptionHandler = CatchAndFixConcurrencyException
            };
            var context = options.CreateAndSeedDbWithDiForHandlers<OrderCreatedHandler>( config: config);
            {
                var stock = context.ProductStocks.OrderBy(x => x.NumInStock).First();

                //ATTEMPT
                stock.NumAllocated = 2;
                context.Database.ExecuteSqlRaw(
                    "UPDATE ProductStocks SET NumAllocated = @p0 WHERE ProductName = @p1", 3, stock.ProductName);
                context.SaveChanges();

                //VERIFY
                var foundStock = context.Find<ProductStock>(stock.ProductName);
                foundStock.NumAllocated.ShouldEqual(5);
            }
        }

        //---------------------------------------------
        // private methods

        private IStatusGeneric CatchAndFixConcurrencyException(Exception ex, DbContext context)
        {
            var dbUpdateEx = ex as DbUpdateConcurrencyException;
            if (dbUpdateEx == null || dbUpdateEx.Entries.Count != 1)
                return null; //can't handle this error

            var entry = dbUpdateEx.Entries.Single();
            if (!(entry.Entity is ProductStock failedUpdate))
                return null;

            var status = new StatusGenericHandler();
            //This entity MUST be read as NoTracking otherwise it will interfere with the same entity we are trying to write
            var overwroteData
                = context.Set<ProductStock>().AsNoTracking().SingleOrDefault(p => p.ProductName == failedUpdate.ProductName);
            if (overwroteData == null)
                //The ProductStock was deleted
                return status.AddError("The product you were interested in has been removed from our stock.");

            var addedChange = failedUpdate.NumAllocated - (int)entry.Property(nameof(ProductStock.NumAllocated)).OriginalValue ;
            var combinedAlloc = overwroteData.NumAllocated + addedChange;

            entry.Property(nameof(ProductStock.NumAllocated)).CurrentValue = combinedAlloc;
            entry.Property(nameof(ProductStock.NumAllocated)).OriginalValue = overwroteData.NumAllocated;

            return status;
        }


    }
}