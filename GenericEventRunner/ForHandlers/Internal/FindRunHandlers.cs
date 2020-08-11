// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using GenericEventRunner.ForSetup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StatusGeneric;

namespace GenericEventRunner.ForHandlers.Internal
{
    internal class FindRunHandlers
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly IGenericEventRunnerConfig _config;

        private FindHandlers _findHandlers;

        public FindRunHandlers(IServiceProvider serviceProvider, ILogger logger, IGenericEventRunnerConfig config)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _config = config;

            _findHandlers = new FindHandlers(serviceProvider, logger);
        }

        /// <summary>
        /// This finds the handlers for the event and runs the handlers with the input event
        /// </summary>
        /// <param name="entityAndEvent"></param>
        /// <param name="loopCount">This gives the loop number for the RunBefore/AfterSaveChangesEvents</param>
        /// <param name="beforeSave">true for BeforeSave, and false for AfterSave</param>
        /// <param name="useAsyncIfFound">True if called by an async </param>
        /// <returns>Returns status with </returns>
        public IStatusGeneric RunHandlersForEvent(EntityAndEvent entityAndEvent, int loopCount, bool beforeSave, bool useAsyncIfFound)
        {
            var status = new StatusGenericHandler
            {
                Message = "Successfully saved."
            };
            var eventType = entityAndEvent.DomainEvent.GetType();

            var handlersAndWrappers = _findHandlers.GetHandlers(eventType, beforeSave, useAsyncIfFound);
            var beforeAfter = beforeSave ? "BeforeSave" : "AfterSave";
            if (!handlersAndWrappers.Any())
            {
                _logger.LogError($"Missing handler for event of type {eventType.FullName} for {beforeAfter} event handler.");
                throw new GenericEventRunnerException(
                    $"Could not find a {beforeAfter} event handler for the event {eventType.Name}.",
                    entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);
            }

            foreach (var handlerWrapper in handlersAndWrappers)
            {

                _logger.LogInformation($"{beforeAfter[0]}{loopCount}: About to run a {beforeAfter} event handler {handlerWrapper.EventHandler.GetType().FullName}.");
                if (beforeSave)
                {
                    var wrappedHandler = (BeforeSaveEventHandler) Activator.CreateInstance(handlerWrapper.WrapperType, handlerWrapper.EventHandler);
                    var handlerStatus = wrappedHandler.Handle(entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);
                    if (handlerStatus != null)
                        status.CombineStatuses(handlerStatus);
                }
                else
                {
                    var wrappedHandler = (AfterSaveEventHandler) Activator.CreateInstance(handlerWrapper.WrapperType, handlerWrapper.EventHandler);
                    wrappedHandler.Handle(entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);
                }
            }

            return status;
        }
        
    }
}