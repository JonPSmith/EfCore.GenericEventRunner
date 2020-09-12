// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace GenericEventRunner.DomainParts
{
    /// <summary>
    /// Add this interface to an entity class to support AfterSaveEvents
    /// </summary>
    public interface IEntityWithAfterSaveEvents
    {
        /// <summary>
        /// This gets all the events in the AfterSaveEvents list, and clears that list at the same time
        /// </summary>
        ICollection<IEntityEvent> GetAfterSaveEventsThenClear();
    }
}