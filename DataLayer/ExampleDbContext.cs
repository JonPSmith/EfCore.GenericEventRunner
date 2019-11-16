// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EntityClasses;
using GenericEventRunner.ForDbContext;
using GenericEventRunner.ForEntities;
using Microsoft.EntityFrameworkCore;

namespace DataLayer
{
    public class ExampleDbContext : DbContext
    {
        private readonly IEventsRunner _eventsRunner;

        public DbSet<Order> Orders { get; set; }
        public DbSet<LineItem> LineItems { get; set; }
        public DbSet<ProductStock> ProductStocks { get; set; }
        public DbSet<TaxRate> TaxRates { get; set; }

        public ExampleDbContext(DbContextOptions<ExampleDbContext> options, IEventsRunner eventRunner = null)
            : base(options)
        {
            _eventsRunner = eventRunner;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductStock>().HasKey(x => x.ProductCode);
        }

        //I only have to override these two version of SaveChanges, as the other two versions call these
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            if (_eventsRunner == null)
                return base.SaveChanges(acceptAllChangesOnSuccess);

            var trackedEntities = ChangeTracker.Entries().ToList();

            return _eventsRunner.RunEventsBeforeAfterSaveChanges(() => ChangeTracker.Entries<EntityEvents>(),
                () => base.SaveChanges(acceptAllChangesOnSuccess));
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_eventsRunner == null)
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);

            return await _eventsRunner.RunEventsBeforeAfterSaveChangesAsync(() => ChangeTracker.Entries<EntityEvents>(),
                () => base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken));
        }
    }
}