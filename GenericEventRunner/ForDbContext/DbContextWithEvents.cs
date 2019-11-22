// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GenericEventRunner.ForEntities;
using GenericEventRunner.ForHandlers;
using Microsoft.EntityFrameworkCore;
using StatusGeneric;

namespace GenericEventRunner.ForDbContext
{
    public class DbContextWithEvents<T> : DbContext where T : DbContext
    {
        private readonly IEventsRunner _eventsRunner;

        public DbContextWithEvents(DbContextOptions<T> options, IEventsRunner eventsRunner) : base(options)
        {
            _eventsRunner = eventsRunner;
        }

        public IStatusGeneric<int> SaveChangesWithStatus(bool acceptAllChangesOnSuccess = true)
        {
            if (_eventsRunner == null)
                throw new GenericEventRunnerException($"The {nameof(SaveChangesWithStatus)} cannot be used unless the event runner is present");

            return _eventsRunner.RunEventsBeforeAfterSaveChanges(() => ChangeTracker.Entries<EntityEvents>(),
                () => base.SaveChanges(acceptAllChangesOnSuccess), false);
        }

        //I only have to override these two version of SaveChanges, as the other two versions call these
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            if (_eventsRunner == null)
                return base.SaveChanges(acceptAllChangesOnSuccess);

            var status = _eventsRunner.RunEventsBeforeAfterSaveChanges(() => ChangeTracker.Entries<EntityEvents>(),
                () => base.SaveChanges(acceptAllChangesOnSuccess), true);

            if (status.IsValid)
                return status.Result;

            throw new GenericEventRunnerException(
                $"Problem when writing to the database: {status.Message}{Environment.NewLine}{status.GetAllErrors()}");
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_eventsRunner == null)
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);

            return await _eventsRunner.RunEventsBeforeAfterSaveChangesAsync(() => ChangeTracker.Entries<EntityEvents>(),
                () => base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken), true);
        }
    }
}