// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace GenericEventRunner.ForEntities
{
    /// <summary>
    /// This is a class that the EF Core entity classes inherit to add events
    /// </summary>
    public abstract class EntityEvents
    {
        //Events are NOT stored in the database - they are transitory events
        //Events are created within a single DBContext and are cleared every time SaveChanges/SaveChangesAsync is called
        
        //This holds events that are run before SaveChanges is called
        private readonly List<IDomainEvent> _beforeSaveEvents = new List<IDomainEvent>();

        //This holds events that are run after SaveChanges finishes successfully
        private readonly List<IDomainEvent> _afterSaveChangesEvents = new List<IDomainEvent>();

        public void AddBeforeSaveEvent(IDomainEvent dEvent)
        {
            _beforeSaveEvents.Add(dEvent);
        }

        public void AddAfterSaveEvent(IDomainEvent dEvent)
        {
            _afterSaveChangesEvents.Add(dEvent);
        }

        public ICollection<IDomainEvent> GetBeforeSaveEventsThenClear()
        {
            var eventCopy = _beforeSaveEvents.ToList();
            _beforeSaveEvents.Clear();
            return eventCopy;
        }

        public ICollection<IDomainEvent> GetAfterSaveEventsThenClear()
        {
            var eventCopy = _afterSaveChangesEvents.ToList();
            _afterSaveChangesEvents.Clear();
            return eventCopy;
        }
    }
}