// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace GenericEventRunner.DomainParts
{
    /// <summary>
    /// This is a class that the EF Core entity classes inherit to add events
    /// </summary>
    public abstract class EntityEventsBase : IEntityWithBeforeSaveEvents, IEntityWithDuringSaveEvents, IEntityWithAfterSaveEvents
    {
        //Events are NOT stored in the database - they are transitory events
        //Events are created within a single DBContext and are cleared every time SaveChanges/SaveChangesAsync is called
        
        //This holds events that are run before SaveChanges is called
        private readonly List<IEntityEvent> _beforeSaveEvents = new List<IEntityEvent>();

        //This holds events that are run within a transaction containing a call to SaveChanges 
        private readonly List<IEntityEvent> _duringSaveEvents = new List<IEntityEvent>();

        //This holds events that are run after SaveChanges finishes successfully
        private readonly List<IEntityEvent> _afterSaveChangesEvents  = new List<IEntityEvent>();

        /// <summary>
        /// This allows an entity to add an event to this class
        /// </summary>
        /// <param name="dEvent">This is the domain event you want to sent</param>
        /// <param name="eventToSend">This allows you to send the event to either BeforeSave, DuringSave or AfterSave. Default is BeforeSave List</param>
        public void AddEvent(IEntityEvent dEvent, EventToSend eventToSend = EventToSend.BeforeSave)
        {
            if (eventToSend == EventToSend.DuringSave)
                _duringSaveEvents.Add(dEvent);
            if (eventToSend == EventToSend.BeforeSave || eventToSend == EventToSend.BeforeAndAfterSave)
                _beforeSaveEvents.Add(dEvent);
            if (eventToSend == EventToSend.AfterSave || eventToSend == EventToSend.BeforeAndAfterSave)
                _afterSaveChangesEvents.Add(dEvent);
        }

        /// <summary>
        /// This gets all the events in the BeforeSaveEvents list, and clears that list at the same time
        /// </summary>
        public ICollection<IEntityEvent> GetBeforeSaveEventsThenClear()
        {
            var eventCopy = _beforeSaveEvents.ToList();
            _beforeSaveEvents.Clear();
            return eventCopy;
        }

        /// <summary>
        /// This returns the events that should be run within a transaction containing a call to SaveChanges
        /// </summary>
        public ICollection<IEntityEvent> GetDuringSaveEvents()
        {
            return _duringSaveEvents;
        }

        /// <summary>
        /// This clears all the during save events once the code within the transaction has finished
        /// </summary>
        public void ClearDuringSaveEvents()
        {
            _duringSaveEvents.Clear();
        }

        /// <summary>
        /// This gets all the events in the AfterSaveEvents list, and clears that list at the same time
        /// </summary>
        public ICollection<IEntityEvent> GetAfterSaveEventsThenClear()
        {
            var eventCopy = _afterSaveChangesEvents.ToList();
            _afterSaveChangesEvents.Clear();
            return eventCopy;
        }

    }
}