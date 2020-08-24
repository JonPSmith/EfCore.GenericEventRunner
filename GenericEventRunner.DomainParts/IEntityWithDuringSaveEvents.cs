// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace GenericEventRunner.DomainParts
{
    /// <summary>
    /// Add this interface to an entity class to support tDuringSaveEvents
    /// </summary>
    public interface IEntityWithDuringSaveEvents
    {
        /// <summary>
        /// This returns the events that should be run within a transaction containing a call to SaveChanges
        /// </summary>
        ICollection<IDomainEvent> GetDuringSaveEvents();

        /// <summary>
        /// This clears all the during save events once the code within the transaction has finished
        /// </summary>
        void ClearDuringSaveEvents();
    }
}