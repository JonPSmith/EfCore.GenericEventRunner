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
    /// <summary>
    /// If you want to add GenericEventsRunner to your DbContext then inherit this instead of DbContext
    /// This overrides the base SaveChanges/SaveChangesAsync to add the event runner before and after the call the base SaveChanges/SaveChangesAsync
    ///
    /// If you don't like inheriting this class then you can copy this code directly into your own DbContext
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DbContextWithEvents<T> : DbContext where T : DbContext
    {
        private readonly IEventsRunner _eventsRunner;

        /// <summary>
        /// This sets up the DbContext options and adds the eventRunner
        /// </summary>
        /// <param name="options">normal EF Core options for a database</param>
        /// <param name="eventsRunner">The Generic Event Runner - can be null which will turn off domain event handling</param>
        protected DbContextWithEvents(DbContextOptions<T> options, IEventsRunner eventsRunner) : base(options)
        {
            _eventsRunner = eventsRunner;
        }

        /// <summary>
        /// This is a spacial form of SaveChanges that returns an <see cref="T:IStatusGeneric{int}"/> status
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess">normal SaveChanges option</param>
        /// <returns>Status, with a Result that is the number of updates down by SaveChanges</returns>
        public IStatusGeneric<int> SaveChangesWithStatus(bool acceptAllChangesOnSuccess = true)
        {
            if (_eventsRunner == null)
                throw new GenericEventRunnerException($"The {nameof(SaveChangesWithStatus)} cannot be used unless the event runner is present");

            return _eventsRunner.RunEventsBeforeAfterSaveChanges(() => ChangeTracker.Entries<EntityEvents>(),
                () => base.SaveChanges(acceptAllChangesOnSuccess), false);
        }

        /// <summary>
        /// This is a spacial form of SaveChangesAsync that returns an <see cref="T:IStatusGeneric{int)"/> status
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Status, with a Result that is the number of updates down by SaveChangesAsync</returns>
        public async Task<IStatusGeneric<int>> SaveChangesWithStatusAsync(bool acceptAllChangesOnSuccess = true,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_eventsRunner == null)
                throw new GenericEventRunnerException($"The {nameof(SaveChangesWithStatusAsync)} cannot be used unless the event runner is present");

            return await _eventsRunner.RunEventsBeforeAfterSaveChangesAsync(() => ChangeTracker.Entries<EntityEvents>(),
                () => base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken), false);
        }

        //I only have to override these two version of SaveChanges, as the other two versions call these
        
        /// <summary>
        /// EF Core's SaveChanges, but with domain event handling added
        /// Throws an exception if any of the BeforeSave event handlers return a status with an error in it.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess"></param>
        /// <returns>number of writes done to the database</returns>
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

        /// <summary>
        /// EF Core's SaveChanges, but with domain event handling added
        /// Throws an exception if any of the BeforeSave event handlers return a status with an error in it.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_eventsRunner == null)
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);

            var status = await _eventsRunner.RunEventsBeforeAfterSaveChangesAsync(() => ChangeTracker.Entries<EntityEvents>(),
                () => base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken), true);

            if (status.IsValid)
                return status.Result;

            throw new GenericEventRunnerException(
                $"Problem when writing to the database: {status.Message}{Environment.NewLine}{status.GetAllErrors()}");
        }
    }
}