// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GenericEventRunner.DomainParts;
using GenericEventRunner.ForSetup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StatusGeneric;

namespace GenericEventRunner.ForHandlers.Internal
{
    internal enum BeforeDuringOrAfter { BeforeSave, DuringBeforeSave, DuringSave, AfterSave }

    internal class RunEachTypeOfEvents
    {
        private readonly FindRunHandlers _findRunHandlers;
        private readonly IGenericEventRunnerConfig _config;

        private List<EntityAndEvent> _duringBeforeEvents;
        private List<EntityAndEvent> _duringAfterEvents;

        public RunEachTypeOfEvents(IServiceProvider serviceProvider, ILogger<EventsRunner> logger, IGenericEventRunnerConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _findRunHandlers = new FindRunHandlers(serviceProvider, logger, _config);
        }

        public bool SetupDuringEvents(DbContext context)
        {
            var duringEvents = new List<EntityAndEvent>();
            foreach (var entityEntry in context.ChangeTracker.Entries<IEntityWithDuringSaveEvents>())
            {
                duringEvents.AddRange(entityEntry.Entity.GetDuringSaveEvents()
                    .Select(x => new EntityAndEvent(entityEntry.Entity, x)));
            }

            if (!duringEvents.Any())
                return false;

            //Find the events marked to run before SaveChanges
            _duringBeforeEvents = duringEvents
                .Where(x => x.DomainEvent.GetType()
                                .GetCustomAttribute<DuringSaveStageAttribute>()?.WhenToExecute ==
                            DuringSaveStages.BeforeSaveChanges)
                .ToList();

            duringEvents.RemoveAll(x => _duringBeforeEvents.Contains(x));
            _duringAfterEvents = duringEvents;

            return true;
        }

        public async ValueTask<IStatusGeneric> RunBeforeSaveChangesEventsAsync(DbContext context, bool allowAsync)
        {
            var status = new StatusGenericHandler();

            //This has to run until there are no new events, because one event might trigger another event
            bool shouldRunAgain;
            int loopCount = 1;
            do
            {
                var eventsToRun = new List<EntityAndEvent>();
                foreach (var entityEntry in context.ChangeTracker.Entries<IEntityWithBeforeSaveEvents>())
                {
                    eventsToRun.AddRange(entityEntry.Entity.GetBeforeSaveEventsThenClear()
                        .Select(x => new EntityAndEvent(entityEntry.Entity, x)));
                }

                shouldRunAgain = false;
                foreach (var entityAndEvent in eventsToRun)
                {
                    shouldRunAgain = true;
                    if (allowAsync)
                        status.CombineStatuses(await _findRunHandlers.RunHandlersForEventAsync(
                                entityAndEvent, loopCount, BeforeDuringOrAfter.BeforeSave, allowAsync)
                            .ConfigureAwait(false));
                    else
                    {
                        var findRunStatus =
                            _findRunHandlers.RunHandlersForEventAsync(entityAndEvent, loopCount, BeforeDuringOrAfter.BeforeSave, allowAsync);
                        if (!findRunStatus.IsCompleted)
                            throw new InvalidOperationException("Can only run sync tasks");
                        status.CombineStatuses(findRunStatus.Result);
                    }
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
                context.ChangeTracker.Entries<IEntityWithBeforeSaveEvents>()
                    .ToList().ForEach(x => x.Entity.GetBeforeSaveEventsThenClear());
                context.ChangeTracker.Entries<IEntityWithAfterSaveEvents>()
                    .ToList().ForEach(x => x.Entity.GetAfterSaveEventsThenClear());

            }

            return status;
        }

        public async ValueTask<IStatusGeneric> RunDuringSaveChangesEventsAsync(DbContext context, bool postSaveChanges, bool allowAsync)
        {
            var status = new StatusGenericHandler();
            var eventType = postSaveChanges
                ? BeforeDuringOrAfter.DuringSave
                : BeforeDuringOrAfter.DuringBeforeSave;
            foreach (var entityAndEvent in postSaveChanges ? _duringAfterEvents : _duringBeforeEvents)
            {
                if (allowAsync)
                    status.CombineStatuses(await _findRunHandlers.RunHandlersForEventAsync(
                            entityAndEvent, 1, eventType, true)
                        .ConfigureAwait(false));
                else
                {
                    var findRunStatus =
                        _findRunHandlers.RunHandlersForEventAsync(entityAndEvent, 1, eventType, false);
                    if (!findRunStatus.IsCompleted)
                        throw new InvalidOperationException("Can only run sync tasks");
                    status.CombineStatuses(findRunStatus.Result);
                }
            }

            return status;
        }


        //NOTE: This had problems throwing an exception (don't know why - RunBeforeSaveChangesEventsAsync work!?).
        //Having it return an exception fixed it
        public async ValueTask<Exception> RunAfterSaveChangesEventsAsync(DbContext context, bool allowAsync)
        {
            var status = new StatusGenericHandler();

            if (_config.NotUsingAfterSaveHandlers)
                //Skip this stage if NotUsingAfterSaveHandlers is true
                return null;

            var eventsToRun = new List<EntityAndEvent>();
            foreach (var entityEntry in context.ChangeTracker.Entries<IEntityWithAfterSaveEvents>())
            {
                eventsToRun.AddRange(entityEntry.Entity.GetAfterSaveEventsThenClear()
                    .Select(x => new EntityAndEvent(entityEntry.Entity, x)));
            }

            foreach (var entityAndEvent in eventsToRun)
            {
                if (allowAsync)
                    status.CombineStatuses(await _findRunHandlers.RunHandlersForEventAsync(entityAndEvent, 1, BeforeDuringOrAfter.AfterSave, allowAsync)
                        .ConfigureAwait(false));
                else
                {
                    var findRunStatus =
                        _findRunHandlers.RunHandlersForEventAsync(entityAndEvent, 1, BeforeDuringOrAfter.AfterSave, allowAsync);
                    if (!findRunStatus.IsCompleted)
                        throw new InvalidOperationException("Can only run sync tasks");
                    status.CombineStatuses(findRunStatus.Result);
                }
            }

            return null;
        }


    }
}