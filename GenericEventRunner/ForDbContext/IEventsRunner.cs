// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GenericEventRunner.ForEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using StatusGeneric;

namespace GenericEventRunner.ForDbContext
{
    /// <summary>
    /// This is the interface for the Events Runner that is in the DbContext
    /// </summary>
    public interface IEventsRunner
    {
        /// <summary>
        /// This Handles the running of the BeforeSave Event Handlers
        /// </summary>
        /// <param name="context"></param>
        /// <param name="getTrackedEntities">Func that gets all the tracked entities that inherited the EntityEvents class</param>
        /// <param name="callBaseSaveChanges">This calls the base SaveChanges.</param>
        /// <returns></returns>
        IStatusGeneric<int> RunEventsBeforeAfterSaveChanges(DbContext context,
            Func<IEnumerable<EntityEntry<EntityEvents>>> getTrackedEntities,
            Func<int> callBaseSaveChanges);

        /// <summary>
        /// This Handles the running of the BeforeSave Event Handlers
        /// </summary>
        /// <param name="context"></param>
        /// <param name="getTrackedEntities">Func that gets all the tracked entities that inherited the EntityEvents class</param>
        /// <param name="callBaseSaveChangesAsync">This calls the base SaveChangesAsync.</param>
        /// <returns></returns>
        Task<IStatusGeneric<int>> RunEventsBeforeAfterSaveChangesAsync(DbContext context,
            Func<IEnumerable<EntityEntry<EntityEvents>>> getTrackedEntities,
            Func<Task<int>> callBaseSaveChangesAsync);
    }
}