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
        private readonly List<IDomainEvent> _afterSaveChangesEvents  = new List<IDomainEvent>();

        /// <summary>
        /// This allows an entity to add an event to this class
        /// </summary>
        /// <param name="dEvent">This is the domain event you want to sent</param>
        /// <param name="eventToSend">This allows you to send the event to either BeforeSave list, the AfterSave list or both. Default is BeforeSave List</param>
        public void AddEvent(IDomainEvent dEvent, EventToSend eventToSend = EventToSend.BeforeSave)
        {
            if (eventToSend == EventToSend.BeforeSave || eventToSend == EventToSend.BeforeAndAfterSave)
                _beforeSaveEvents.Add(dEvent);
            if (eventToSend == EventToSend.AfterSave || eventToSend == EventToSend.BeforeAndAfterSave)
                _afterSaveChangesEvents.Add(dEvent);
        }

        /// <summary>
        /// This gets all the events in the BeforeSaveEvents list, and clears that list at the same time
        /// </summary>
        /// <returns></returns>
        public ICollection<IDomainEvent> GetBeforeSaveEventsThenClear()
        {
            var eventCopy = _beforeSaveEvents.ToList();
            _beforeSaveEvents.Clear();
            return eventCopy;
        }

        /// <summary>
        /// This gets all the events in the AfterSaveEvents list, and clears that list at the same time
        /// </summary>
        /// <returns></returns>
        public ICollection<IDomainEvent> GetAfterSaveEventsThenClear()
        {
            var eventCopy = _afterSaveChangesEvents.ToList();
            _afterSaveChangesEvents.Clear();
            return eventCopy;
        }
    }
}