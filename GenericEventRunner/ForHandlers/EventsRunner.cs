// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericEventRunner.ForDbContext;
using GenericEventRunner.ForEntities;
using GenericEventRunner.ForHandlers.Internal;
using GenericEventRunner.ForSetup;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace GenericEventRunner.ForHandlers
{
    public class EventsRunner : IEventsRunner
    {
        private readonly FindRunHandlers _findRunHandlers;
        private readonly ILogger _logger;
        private readonly GenericEventRunnerConfig _config;

        public EventsRunner(IServiceProvider serviceProvider, ILogger<EventsRunner> logger, GenericEventRunnerConfig config = null)
        {
            _config = config ?? new GenericEventRunnerConfig();
            _logger = logger;
            _findRunHandlers = new FindRunHandlers(serviceProvider, _logger, _config);
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
            int numTimesAround = 0;
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
                    _findRunHandlers.RunHandlersForEvent(entityAndEvent, true);
                }
                if (++numTimesAround > _config.MaxTimesToLookForBeforeEvents)
                    throw new GenericEventRunnerException(
                        $"The BeforeSave event loop exceeded the config's {nameof(GenericEventRunnerConfig.MaxTimesToLookForBeforeEvents)}" +
                        $" value of {_config.MaxTimesToLookForBeforeEvents}. This implies a circular sets of events.",
                        eventsToRun.Last().CallingEntity, eventsToRun.Last().DomainEvent);
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
                    _findRunHandlers.RunHandlersForEvent(entityAndEvent, true);
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
                _findRunHandlers.RunHandlersForEvent(entityAndEvent, false);
            }
        }

    }
}