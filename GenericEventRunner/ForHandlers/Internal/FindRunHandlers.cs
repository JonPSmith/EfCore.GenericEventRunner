// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
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

        public FindRunHandlers(IServiceProvider serviceProvider, ILogger logger, IGenericEventRunnerConfig config)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _config = config;
        }

        /// <summary>
        /// This finds the handlers for the event and runs the handlers with the input event
        /// </summary>
        /// <param name="entityAndEvent"></param>
        /// <param name="loopCount">This gives the loop number for the RunBefore/AfterSaveChangesEvents</param>
        /// <param name="beforeSave">true for BeforeSave, and false for AfterSave</param>
        /// <param name="dontConvertExToStatus">If true then exceptions should be re-thrown</param>
        public IStatusGeneric RunHandlersForEvent(EntityAndEvent entityAndEvent, int loopCount, bool beforeSave, bool dontConvertExToStatus)
        {
            var status = new StatusGenericHandler
            {
                Message = "Successfully saved."
            };

            var eventType = entityAndEvent.DomainEvent.GetType();
            var handlerInterface = (beforeSave ? typeof(IBeforeSaveEventHandler<>) : typeof(IAfterSaveEventHandler<>))
                .MakeGenericType(eventType);
            var wrapperType = (beforeSave ? typeof(BeforeSaveHandler<>) : typeof(AfterSaveHandler<>))
                .MakeGenericType(eventType);
            var handlers = _serviceProvider.GetServices(handlerInterface).ToList();

            var beforeAfter = beforeSave ? "BeforeSave" : "AfterSave";
            if (!handlers.Any())
            {
                _logger.LogError($"Missing handler for event of type {eventType.FullName} for {beforeAfter} event handler.");
                throw new GenericEventRunnerException(
                    $"Could not find a {beforeAfter} event handler for the event {eventType.Name}.",
                    entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);
            }

            foreach (var handler in handlers)
            {

                _logger.LogInformation($"{beforeAfter[0]}{loopCount}: About to run a {beforeAfter} event handler {handler.GetType().FullName}.");
                if (beforeSave)
                {
                    var wrappedHandler = (BeforeSaveEventHandler) Activator.CreateInstance(wrapperType, handler);
                    var handlerStatus = wrappedHandler.Handle(entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);
                    if (handlerStatus != null)
                        status.CombineStatuses(handlerStatus);
                }
                else
                {
                    var wrappedHandler = (AfterSaveEventHandler) Activator.CreateInstance(wrapperType, handler);
                    wrappedHandler.Handle(entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);
                }
            }

            return status;
        }
        
    }
}