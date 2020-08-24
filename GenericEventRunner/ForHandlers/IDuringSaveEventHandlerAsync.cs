// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using GenericEventRunner.DomainParts;
using StatusGeneric;

namespace GenericEventRunner.ForHandlers
{
    /// <summary>
    /// Place this on any event handler that should be called within a transaction containing a call to SaveChanges
    /// </summary>
    /// <typeparam name="T">This should be the domain event that this handler is looking for</typeparam>
    public interface IDuringSaveEventHandlerAsync<in T> where T : IDomainEvent
    {
        /// <summary>
        /// This is the method you must define to produce a AfterSave event handler 
        /// </summary>
        /// <param name="callingEntity"></param>
        /// <param name="domainEvent"></param>
        /// <param name="uniqueKey">A unique value per transaction. This allows you to detect retries of transactions</param>
        /// <returns>You must return a IStatusGeneric. If has an error the transaction will be rolled back</returns>
        Task<IStatusGeneric> HandleAsync(object callingEntity, T domainEvent, Guid uniqueKey);
    }
}