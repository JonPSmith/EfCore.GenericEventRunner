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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using StatusGeneric;

namespace GenericEventRunner.ForHandlers
{
    /// <summary>
    /// This is the class that will be injected into the DbContext to handle events
    /// NOTE: The EventsRunner has the lifetime of the DbContext, i.e. its Scoped
    /// </summary>
    public class EventsRunner : IEventsRunner
    {
        private readonly FindRunHandlers _findRunHandlers;
        private readonly IGenericEventRunnerConfig _config;

        /// <summary>
        /// This is the class that will manage the events inside your DbContext
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        public EventsRunner(IServiceProvider serviceProvider, ILogger<EventsRunner> logger, IGenericEventRunnerConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _findRunHandlers = new FindRunHandlers(serviceProvider, logger, _config);
        }

        /// <summary>
        /// This runs the events before and after the base SaveChanges method is run
        /// </summary>
        /// <param name="context">The current DbContext</param>
        /// <param name="getTrackedEntities">A function to get the tracked entities</param>
        /// <param name="callBaseSaveChanges">A function that is linked to the base SaveChanges in your DbContext</param>
        /// <returns>Returns the status with the numUpdated number from SaveChanges</returns>
        public IStatusGeneric<int> RunEventsBeforeAfterSaveChanges(DbContext context, Func<IEnumerable<EntityEntry<EntityEvents>>> getTrackedEntities,  
            Func<int> callBaseSaveChanges)
        {
            var status = new StatusGenericHandler<int>();
            status.CombineStatuses(RunBeforeSaveChangesEvents(getTrackedEntities));
            if (!status.IsValid) 
                return status;

            //Call SaveChanges with catch for exception handler
            do
            {
                try
                {
                    status.SetResult(callBaseSaveChanges.Invoke());
                    break; //This breaks out of the do/while
                }
                catch (Exception e)
                {
                    var exceptionStatus = _config.SaveChangesExceptionHandler?.Invoke(e, context);
                    if (exceptionStatus == null)
                        //This means the SaveChangesExceptionHandler doesn't cover this type of Concurrency Exception
                        throw;
                    //SaveChangesExceptionHandler ran, so combine its error into the outer status
                    status.CombineStatuses(exceptionStatus);
                }
                //If the SaveChangesExceptionHandler fixed the problem then we call SaveChanges again, but with the same exception catching.
            } while (status.IsValid);

            RunAfterSaveChangesEvents(getTrackedEntities);
            return status;
        }

        /// <summary>
        /// This runs the events before and after the base SaveChangesAsync method is run
        /// </summary>
        /// <param name="context">The current DbContext</param>
        /// <param name="getTrackedEntities">A function to get the tracked entities</param>
        /// <param name="callBaseSaveChangesAsync">A function that is linked to the base SaveChangesAsync in your DbContext</param>
        /// <returns>Returns the status with the numUpdated number from SaveChanges</returns>
        public async Task<IStatusGeneric<int>> RunEventsBeforeAfterSaveChangesAsync(DbContext context, Func<IEnumerable<EntityEntry<EntityEvents>>> getTrackedEntities, 
            Func<Task<int>> callBaseSaveChangesAsync)
        {
            var status = new StatusGenericHandler<int>();
            status.CombineStatuses(RunBeforeSaveChangesEvents(getTrackedEntities));
            if (!status.IsValid)
                return status;

            //Call SaveChangesAsync with catch for exception handler
            do
            {
                try
                {
                    status.SetResult(await callBaseSaveChangesAsync.Invoke().ConfigureAwait(false));
                    break; //This breaks out of the do/while
                }
                catch (Exception e)
                {
                    var exceptionStatus = _config.SaveChangesExceptionHandler?.Invoke(e, context);
                    if (exceptionStatus == null)
                        //This means the SaveChangesExceptionHandler doesn't cover this type of Concurrency Exception
                        throw;
                    //SaveChangesExceptionHandler ran, so combine its error into the outer status
                    status.CombineStatuses(exceptionStatus);
                }
                //If the SaveChangesExceptionHandler fixed the problem then we call SaveChanges again, but with the same exception catching.
            } while (status.IsValid);
            RunAfterSaveChangesEvents(getTrackedEntities);
            return status;
        }

        //------------------------------------------
        // private methods

        private IStatusGeneric RunBeforeSaveChangesEvents(Func<IEnumerable<EntityEntry<EntityEvents>>> getTrackedEntities)
        {
            var status = new StatusGenericHandler();

            //This has to run until there are no new events, because one event might trigger another event
            bool shouldRunAgain;
            int loopCount = 1;
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
                    status.CombineStatuses( _findRunHandlers.RunHandlersForEvent(entityAndEvent, loopCount, true));
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
                //If errors then clear any extra before/after events.
                //We need to do this to ensure another call to SaveChanges doesn't get the old events
                getTrackedEntities.Invoke().ToList().ForEach(x =>
                {
                    x.Entity.GetBeforeSaveEventsThenClear();
                    x.Entity.GetAfterSaveEventsThenClear();
                });

            return status;
        }

        private void RunAfterSaveChangesEvents(Func<IEnumerable<EntityEntry<EntityEvents>>> getTrackedEntities)
        {
            if (_config.NotUsingAfterSaveHandlers)
                //Skip this stage if NotUsingAfterSaveHandlers is true
                return;

            var eventsToRun = new List<EntityAndEvent>();
            foreach (var entity in getTrackedEntities.Invoke().Select(x => x.Entity))
            {
                eventsToRun.AddRange(entity.GetAfterSaveEventsThenClear()
                    .Select(x => new EntityAndEvent(entity, x)));
            }

            foreach (var entityAndEvent in eventsToRun)
            {
                _findRunHandlers.RunHandlersForEvent(entityAndEvent, 1, false);
            }
        }

    }
}