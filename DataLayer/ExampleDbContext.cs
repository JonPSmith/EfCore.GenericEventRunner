// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using EntityClasses;
using GenericEventRunner.ForDbContext;
using Microsoft.EntityFrameworkCore;

namespace DataLayer
{
    public class ExampleDbContext : DbContextWithEvents<ExampleDbContext>
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<LineItem> LineItems { get; set; }
        public DbSet<ProductStock> ProductStocks { get; set; }
        public DbSet<TaxRate> TaxRates { get; set; }

        public ExampleDbContext(DbContextOptions<ExampleDbContext> options, IEventsRunner eventRunner = null)
            : base(options, eventRunner)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductStock>().HasKey(x => x.ProductName);
        }
    }
}