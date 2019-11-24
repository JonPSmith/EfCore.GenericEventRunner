// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GenericEventRunner.ForEntities;
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
        /// This Hanlses 
        /// </summary>
        /// <param name="getTrackedEntities"></param>
        /// <param name="callBaseSaveChanges"></param>
        /// <returns></returns>
        IStatusGeneric<int> RunEventsBeforeAfterSaveChanges(Func<IEnumerable<EntityEntry<EntityEvents>>> getTrackedEntities,  
            Func<int> callBaseSaveChanges);

        Task<IStatusGeneric<int>> RunEventsBeforeAfterSaveChangesAsync(Func<IEnumerable<EntityEntry<EntityEvents>>> getTrackedEntities, 
            Func<Task<int>> callBaseSaveChangesAsync);
    }
}