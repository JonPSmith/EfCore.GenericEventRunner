// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace GenericEventRunner.DomainParts
{
    /// <summary>
    /// Add this interface to an entity class to support BeforeSaveEvents
    /// </summary>
    public interface IEntityWithBeforeSaveEvents
    {
        /// <summary>
        /// This gets all the events in the BeforeSaveEvents list, and clears that list at the same time
        /// </summary>
        ICollection<IEntityEvent> GetBeforeSaveEventsThenClear();
    }
}