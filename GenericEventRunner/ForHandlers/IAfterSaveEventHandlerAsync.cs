// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading.Tasks;
using GenericEventRunner.DomainParts;

namespace GenericEventRunner.ForHandlers
{
    /// <summary>
    /// Place this on any async event handler that should be called after SaveChanges has updated the database
    /// </summary>
    /// <typeparam name="T">This should be the domain event that this handler is looking for</typeparam>
    public interface IAfterSaveEventHandlerAsync<in T> where T : IDomainEvent
    {
        /// <summary>
        /// This is the method you must define to produce a AfterSave event handler 
        /// </summary>
        /// <param name="callingEntity"></param>
        /// <param name="domainEvent"></param>
        Task HandleAsync(EntityEventsBase callingEntity, T domainEvent);
    }
}