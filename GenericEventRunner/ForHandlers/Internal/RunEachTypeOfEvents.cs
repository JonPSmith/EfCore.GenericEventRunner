﻿// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericEventRunner.DomainParts;
using GenericEventRunner.ForSetup;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using StatusGeneric;

namespace GenericEventRunner.ForHandlers.Internal
{
    internal class RunEachTypeOfEvents
    {
        private readonly FindRunHandlers _findRunHandlers;
        private readonly IGenericEventRunnerConfig _config;

        public RunEachTypeOfEvents(IServiceProvider serviceProvider, ILogger<EventsRunner> logger, IGenericEventRunnerConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _findRunHandlers = new FindRunHandlers(serviceProvider, logger, _config);
        }

        public async ValueTask<IStatusGeneric> RunBeforeSaveChangesEventsAsync(
            Func<IEnumerable<EntityEntry>> getTrackedEntities, bool allowAsync)
        {
            var status = new StatusGenericHandler();

            //This has to run until there are no new events, because one event might trigger another event
            bool shouldRunAgain;
            int loopCount = 1;
            do
            {
                var eventsToRun = new List<EntityAndEvent>();
                foreach (var entity in getTrackedEntities().Select(x => x.Entity).OfType<IEntityWithBeforeSaveEvents>())
                {
                    eventsToRun.AddRange(entity.GetBeforeSaveEventsThenClear()
                        .Select(x => new EntityAndEvent(entity, x)));
                }

                shouldRunAgain = false;
                foreach (var entityAndEvent in eventsToRun)
                {
                    shouldRunAgain = true;
                    status.CombineStatuses(await _findRunHandlers.RunHandlersForEventAsync(entityAndEvent, loopCount, true, allowAsync)
                        .ConfigureAwait(false));
                    if (!status.IsValid && _config.StopOnFirstBeforeHandlerThatHasAnError)
                        break;
                }
                if (loopCount++ > _config.MaxTimesToLookForBeforeEvents)
                    throw new GenericEventRunnerException(
                        $"The BeforeSave event loop exceeded the config's {nameof(GenericEventRunnerConfig.MaxTimesToLookForBeforeEvents)}" +
                        $" value of {_config.MaxTimesToLookForBeforeEvents}. This implies a circular sets of events. " +
                        "Look at EventsRunner Information logs for more information on what event handlers were running.",
                        eventsToRun.Last().CallingEntity, eventsToRun.Last().DomainEvent);
            } while (shouldRunAgain && (status.IsValid || !_config.StopOnFirstBeforeHandlerThatHasAnError));

            if (!status.IsValid)
            {
                //If errors then clear any extra before/after events.
                //We need to do this to ensure another call to SaveChanges doesn't get the old events
                getTrackedEntities().Select(x => x.Entity).OfType<IEntityWithBeforeSaveEvents>()
                    .ToList().ForEach(x => x.GetBeforeSaveEventsThenClear());
                getTrackedEntities().Select(x => x.Entity).OfType<IEntityWithAfterSaveEvents>()
                    .ToList().ForEach(x => x.GetAfterSaveEventsThenClear());

            }

            return status;
        }

        public async ValueTask<IStatusGeneric> RunDuringSaveChangesEventsAsync(
            Func<IEnumerable<EntityEntry>> getTrackedEntities, bool beforeSavChanges, bool allowAsync)
        {
            return new StatusGenericHandler();
        }


        //NOTE: This had problems throwing an exception (don't know why - RunBeforeSaveChangesEventsAsync work!?).
        //Having it return an exception fixed it
        public async ValueTask<Exception> RunAfterSaveChangesEventsAsync(
            Func<IEnumerable<EntityEntry>> getTrackedEntities, bool allowAsync)
        {
            if (_config.NotUsingAfterSaveHandlers)
                //Skip this stage if NotUsingAfterSaveHandlers is true
                return null;

            var eventsToRun = new List<EntityAndEvent>();
            foreach (var entity in getTrackedEntities().Select(x => x.Entity).OfType<IEntityWithAfterSaveEvents>())
            {
                eventsToRun.AddRange(entity.GetAfterSaveEventsThenClear()
                    .Select(x => new EntityAndEvent(entity, x)));
            }

            foreach (var entityAndEvent in eventsToRun)
            {
                await _findRunHandlers.RunHandlersForEventAsync(entityAndEvent, 1, false, allowAsync).ConfigureAwait(false);
            }

            return null;
        }
    }
}