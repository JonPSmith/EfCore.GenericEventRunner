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
        internal List<IDomainEvent> BeforeSaveEvents { get; private set; } = new List<IDomainEvent>();

        //This holds events that are run after SaveChanges finishes successfully
        internal List<IDomainEvent> AfterSaveChangesEvents { get; private set; } = new List<IDomainEvent>();

        public void AddEvent(IDomainEvent dEvent, EventToSend eventToSend = EventToSend.Before)
        {
            if (eventToSend == EventToSend.Before || eventToSend == EventToSend.Both)
                BeforeSaveEvents.Add(dEvent);
            if (eventToSend == EventToSend.After || eventToSend == EventToSend.Both)
                AfterSaveChangesEvents.Add(dEvent);
        }

        public ICollection<IDomainEvent> GetBeforeSaveEventsThenClear()
        {
            var eventCopy = BeforeSaveEvents.ToList();
            BeforeSaveEvents.Clear();
            return eventCopy;
        }

        public ICollection<IDomainEvent> GetAfterSaveEventsThenClear()
        {
            var eventCopy = AfterSaveChangesEvents.ToList();
            AfterSaveChangesEvents.Clear();
            return eventCopy;
        }
    }
}