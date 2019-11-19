// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericEventRunner.ForSetup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenericEventRunner.ForHandlers.Internal
{
    internal class FindRunHandlers
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly GenericEventRunnerConfig _config;

        public FindRunHandlers(IServiceProvider serviceProvider, ILogger logger, GenericEventRunnerConfig config)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _config = config;
        }


        /// <summary>
        /// This finds the handlers for the event and runs the handlers with the input event
        /// </summary>
        /// <param name="entityAndEvent"></param>
        /// <param name="beforeSave">true for BeforeSave, and false for AfterSave</param>
        public void RunHandlersForEvent(EntityAndEvent entityAndEvent, bool beforeSave)
        {
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
                if (!_config.DoNotThrowExceptionIfNoHandlerForAnEvent)
                    throw new GenericEventRunnerException(
                        $"Could not find a {beforeAfter} event handler for the event {eventType.Name}.",
                        entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);
            }

            foreach (var handler in handlers)
            {
                _logger.LogInformation($"About to run a {beforeAfter} event handler {handler.GetType().FullName}.");
                if (beforeSave)
                {
                    var wrappedHandler = (BeforeSaveEventHandler)Activator.CreateInstance(wrapperType, handler);
                    wrappedHandler.Handle(entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);
                }
                else
                {
                    var wrappedHandler = (AfterSaveEventHandler)Activator.CreateInstance(wrapperType, handler);
                    wrappedHandler.Handle(entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);
                }
            }
        }
        
    }
}