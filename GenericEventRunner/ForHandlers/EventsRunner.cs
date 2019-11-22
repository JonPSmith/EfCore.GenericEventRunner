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
using StatusGeneric;

namespace GenericEventRunner.ForHandlers
{
    /// <summary>
    /// The EventsRunner has the lifetime of the DbContext, i.e. its Scoped
    /// </summary>
    public class EventsRunner : IEventsRunner
    {
        private readonly FindRunHandlers _findRunHandlers;
        private readonly IGenericEventRunnerConfig _config;

        public EventsRunner(IServiceProvider serviceProvider, ILogger<EventsRunner> logger, IGenericEventRunnerConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _findRunHandlers = new FindRunHandlers(serviceProvider, logger, _config);
        }

        public IStatusGeneric<int> RunEventsBeforeAfterSaveChanges(Func<IEnumerable<EntityEntry<EntityEvents>>> getTrackedEntities,  
            Func<int> callBaseSaveChanges, bool nonStatusCall)
        {
            var status = new StatusGenericHandler<int>();
            status.CombineStatuses(RunBeforeSaveChangesEvents(getTrackedEntities, nonStatusCall));
            if (!status.IsValid) 
                return status;

            status.Message = "Successfully saved";
            status.SetResult(callBaseSaveChanges.Invoke());
            RunAfterSaveChangesEvents(getTrackedEntities, nonStatusCall);
            return status;
        }

        public async Task<int> RunEventsBeforeAfterSaveChangesAsync(Func<IEnumerable<EntityEntry<EntityEvents>>> getTrackedEntities, 
            Func<Task<int>> callBaseSaveChangesAsync, bool nonStatusCall)
        {
            RunBeforeSaveChangesEvents(getTrackedEntities, nonStatusCall);
            var numChanges = await callBaseSaveChangesAsync.Invoke().ConfigureAwait(false);
            RunAfterSaveChangesEvents(getTrackedEntities, nonStatusCall);
            return numChanges;
        }


        private IStatusGeneric RunBeforeSaveChangesEvents(Func<IEnumerable<EntityEntry<EntityEvents>>> getTrackedEntities, bool dontConvertExToStatus)
        {
            var status = new StatusGenericHandler();

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
                    status.CombineStatuses( _findRunHandlers.RunHandlersForEvent(entityAndEvent, true, dontConvertExToStatus));
                    if (!status.IsValid)
                        break;
                }
                if (++numTimesAround > _config.MaxTimesToLookForBeforeEvents)
                    throw new GenericEventRunnerException(
                        $"The BeforeSave event loop exceeded the config's {nameof(GenericEventRunnerConfig.MaxTimesToLookForBeforeEvents)}" +
                        $" value of {_config.MaxTimesToLookForBeforeEvents}. This implies a circular sets of events.",
                        eventsToRun.Last().CallingEntity, eventsToRun.Last().DomainEvent);
            } while (shouldRunAgain && status.IsValid);

            if (!status.IsValid)
                //If errors then clear any extra before/after events.
                //We need to do this to ensure another call to SaveChanges doesn't get the old events
                getTrackedEntities.Invoke().ToList().ForEach(x =>
                {
                    x.Entity.GetBeforeSaveEventsThenClear();
                    x.Entity.GetAfterSaveEventsThenClear();
                });

            return status;
        }

        private async Task RunBeforeSaveChangesEventsAsync(Func<IEnumerable<EntityEntry<EntityEvents>>> getTrackedEntities, bool dontConvertExToStatus)
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
                    _findRunHandlers.RunHandlersForEvent(entityAndEvent, true, dontConvertExToStatus);
                }
            } while (shouldRunAgain);
        }

        private void RunAfterSaveChangesEvents(Func<IEnumerable<EntityEntry<EntityEvents>>> getTrackedEntities, bool dontConvertExToStatus)
        {
            var eventsToRun = new List<EntityAndEvent>();
            foreach (var entity in getTrackedEntities.Invoke().Select(x => x.Entity))
            {
                eventsToRun.AddRange(entity.GetAfterSaveEventsThenClear()
                    .Select(x => new EntityAndEvent(entity, x)));
            }

            foreach (var entityAndEvent in eventsToRun)
            { 
                _findRunHandlers.RunHandlersForEvent(entityAndEvent, false, dontConvertExToStatus);
            }
        }

    }
}