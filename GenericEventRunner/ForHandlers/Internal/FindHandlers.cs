// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenericEventRunner.ForHandlers.Internal
{
    internal class FindHandlers
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public FindHandlers(IServiceProvider serviceProvider, ILogger logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public List<HandlerAndWrapper> GetHandlers(EntityAndEvent entityAndEvent, bool beforeSave, bool lookForAsyncHandlers)
        {
            var eventType = entityAndEvent.DomainEvent.GetType();
            var asyncHandlers = new List<object>();
            if (lookForAsyncHandlers)
            {
                var asyncHandlerInterface = (beforeSave ? typeof(IBeforeSaveEventHandlerAsync<>) : typeof(IAfterSaveEventHandlerAsync<>))
                    .MakeGenericType(eventType);
                asyncHandlers = _serviceProvider.GetServices(asyncHandlerInterface).ToList();
            }
            var syncHandlerInterface = (beforeSave ? typeof(IBeforeSaveEventHandler<>) : typeof(IAfterSaveEventHandler<>))
                .MakeGenericType(eventType);
            var syncHandler = _serviceProvider.GetServices(syncHandlerInterface)
                //This removes sync event handlers that have the same name 
                .Where(x => asyncHandlers.All( y => 
                    !string.Equals(x.GetType().Name + "Async", y.GetType().Name, StringComparison.InvariantCultureIgnoreCase)))
                    .ToList();

            var result = asyncHandlers.Select(x => new HandlerAndWrapper(x, eventType, beforeSave, true))
                .Union(syncHandler.Select(x => new HandlerAndWrapper(x, eventType, beforeSave, false))).ToList();

            if (!result.Any())
            {
                var beforeAfter = beforeSave ? "BeforeSave" : "AfterSave";
                _logger.LogError($"Missing handler for event of type {eventType.FullName} for {beforeAfter} event handler.");
                throw new GenericEventRunnerException(
                    $"Could not find a {beforeAfter} event handler for the event {eventType.Name}.",
                    entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);
            }

            return result;
        }
    }
}