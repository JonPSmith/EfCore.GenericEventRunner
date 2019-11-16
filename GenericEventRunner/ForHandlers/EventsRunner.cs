// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericEventRunner.ForDbContext;
using GenericEventRunner.ForEntities;
using GenericEventRunner.ForHandlers.Internal;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace GenericEventRunner.ForHandlers
{
    public class EventsRunner : IEventsRunner
    {
        private readonly FindRunHandlers _findRunHandlers;

        public EventsRunner(IServiceProvider serviceProvider)
        {
            _findRunHandlers = new FindRunHandlers(serviceProvider);
        }

        public int RunEventsBeforeAfterSaveChanges(Func<IEnumerable<EntityEntry<EntityEvents>>> getTrackedEntities,  
            Func<int> callBaseSaveChanges)
        {
            RunBeforeSaveChangesEvents(getTrackedEntities);
            var numChanges = callBaseSaveChanges.Invoke();
            RunAfterSaveChangesEvents(getTrackedEntities);
            return numChanges;
        }

        public async Task<int> RunEventsBeforeAfterSaveChangesAsync(Func<IEnumerable<EntityEntry<EntityEvents>>> getTrackedEntities, 
            Func<Task<int>> callBaseSaveChangesAsync)
        {
            RunBeforeSaveChangesEvents(getTrackedEntities);
            var numChanges = await callBaseSaveChangesAsync.Invoke().ConfigureAwait(false);
            RunAfterSaveChangesEvents(getTrackedEntities);
            return numChanges;
        }


        private void RunBeforeSaveChangesEvents(Func<IEnumerable<EntityEntry<EntityEvents>>> getTrackedEntities)
        {
            //This has to run until there are no new events, because one event might trigger another event
            bool shouldRunAgain;
            do
            {
                var eventsToRun = new List<EntityAndEvent>();
                foreach (var entity in getTrackedEntities.Invoke().Select(x => x.Entity))
                {
                    eventsToRun.AddRange(entity.GetBeforeSaveEventsThenClear()
                        .Select(x => new EntityAndEvent(entity, x)));
                }

                shouldRunAgain = false;
                foreach (var entityAndEvent in eventsToRun)
                {
                    shouldRunAgain = true;
                    _findRunHandlers.DispatchBeforeSave(entityAndEvent);
                }
            } while (shouldRunAgain);
        }

        private async Task RunBeforeSaveChangesEventsAsync(Func<IEnumerable<EntityEntry<EntityEvents>>> getTrackedEntities)
        {
            //This has to run until there are no new events, because one event might trigger another event
            bool shouldRunAgain;
            do
            {
                var eventsToRun = new List<EntityAndEvent>();
                foreach (var entity in getTrackedEntities.Invoke().Select(x => x.Entity))
                {
                    eventsToRun.AddRange(entity.GetBeforeSaveEventsThenClear()
                        .Select(x => new EntityAndEvent(entity, x)));
                }

                shouldRunAgain = false;
                foreach (var entityAndEvent in eventsToRun)
                {
                    shouldRunAgain = true;
                    _findRunHandlers.DispatchBeforeSave(entityAndEvent);
                }
            } while (shouldRunAgain);
        }

        private void RunAfterSaveChangesEvents(Func<IEnumerable<EntityEntry<EntityEvents>>> getTrackedEntities)
        {
            var eventsToRun = new List<EntityAndEvent>();
            foreach (var entity in getTrackedEntities.Invoke().Select(x => x.Entity))
            {
                eventsToRun.AddRange(entity.GetAfterSaveEventsThenClear()
                    .Select(x => new EntityAndEvent(entity, x)));
            }

            foreach (var entityAndEvent in eventsToRun)
            { 
                _findRunHandlers.DispatchAfterSave(entityAndEvent);
            }
        }

    }
}