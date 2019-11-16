// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using EntityClasses;
using Microsoft.EntityFrameworkCore;

namespace DataLayer
{
    public class ExampleDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<LineItem> LineItems { get; set; }
        public DbSet<ProductStock> ProductStocks { get; set; }

        public ExampleDbContext(DbContextOptions<ExampleDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductStock>().HasKey(x => x.ProductCode);
        }
    }
}