// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using GenericEventRunner.DomainParts;
using StatusGeneric;

namespace GenericEventRunner.ForHandlers
{
    /// <summary>
    /// Place this on any event handler that should be called before SaveChanges has updated the database
    /// </summary>
    /// <typeparam name="T">This should be the domain event that this handler is looking for</typeparam>
    public interface IBeforeSaveEventHandler<in T> where T : IEntityEvent
    {
        /// <summary>
        /// This is the method you must define to produce a BeforeSave event handler 
        /// </summary>
        /// <param name="callingEntity"></param>
        /// <param name="domainEvent"></param>
        /// <returns>This can be null if you don't want to return a status, otherwise it should be a IStatusGeneric type</returns>
        IStatusGeneric Handle(object callingEntity, T domainEvent);
    }
}