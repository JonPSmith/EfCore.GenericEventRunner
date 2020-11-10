// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
        /// <param name="callBaseSaveChanges">This calls the base SaveChanges.</param>
        /// <returns></returns>
        IStatusGeneric<int> RunEventsBeforeDuringAfterSaveChanges(DbContext context,
            Func<int> callBaseSaveChanges);

        /// <summary>
        /// This Handles the running of the BeforeSave Event Handlers
        /// </summary>
        /// <param name="context"></param>
        /// <param name="callBaseSaveChangesAsync">This calls the base SaveChangesAsync.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IStatusGeneric<int>> RunEventsBeforeDuringAfterSaveChangesAsync(DbContext context,
            Func<Task<int>> callBaseSaveChangesAsync, CancellationToken cancellationToken);
    }
}