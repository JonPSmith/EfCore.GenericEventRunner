// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Schema;
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

        public List<HandlerAndWrapper> GetHandlers(EntityAndEvent entityAndEvent, BeforeDuringOrAfter beforeDuringOrAfter, bool lookForAsyncHandlers)
        {
            var eventType = entityAndEvent.DomainEvent.GetType();
            var asyncHandlers = new List<object>();

            List<object> GetAsyncHandlers()
            {
                var asyncHandlerInterface = GetEventHandlerGenericType(beforeDuringOrAfter, true)
                    .MakeGenericType(eventType);
                asyncHandlers = _serviceProvider.GetServices(asyncHandlerInterface).ToList();
                return asyncHandlers;
            }

            if(lookForAsyncHandlers)
            {
                asyncHandlers = GetAsyncHandlers();
            }
            var syncHandlerInterface = GetEventHandlerGenericType(beforeDuringOrAfter, false)
                .MakeGenericType(eventType);

            var syncHandler = _serviceProvider.GetServices(syncHandlerInterface)
                //This removes sync event handlers that have the same name 
                .Where(x => asyncHandlers.All(y =>
                    !string.Equals(x.GetType().Name + "Async", y.GetType().Name, StringComparison.InvariantCultureIgnoreCase)))
                .ToList();

            var result = asyncHandlers.Select(x => new HandlerAndWrapper(x, eventType, beforeDuringOrAfter, true))
                .Union(syncHandler.Select(x => new HandlerAndWrapper(x, eventType, beforeDuringOrAfter, false))).ToList();

            if (!result.Any())
            {
                var suffix = GetAsyncHandlers().Any() ? " Their was a suitable async event handler available, but you didn't call SaveChangesAsync." : "";
                _logger.LogError($"Missing handler for event of type {eventType.FullName} for {beforeDuringOrAfter} event handler.{suffix}");
                throw new GenericEventRunnerException(
                    $"Could not find a {beforeDuringOrAfter} event handler for the event {eventType.Name}.{suffix}",
                    entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);
            }

            return result;
        }

        private Type GetEventHandlerGenericType(BeforeDuringOrAfter beforeDuringOrAfter, bool lookForAsyncHandlers)
        {
            switch (beforeDuringOrAfter, lookForAsyncHandlers)
            {
                case (BeforeDuringOrAfter.BeforeSave, false):
                    return typeof(IBeforeSaveEventHandler<>);
                case (BeforeDuringOrAfter.BeforeSave, true):
                    return typeof(IBeforeSaveEventHandlerAsync<>);
                case (BeforeDuringOrAfter.DuringBeforeSave, false):
                case (BeforeDuringOrAfter.DuringSave, false):
                    return typeof(IDuringSaveEventHandler<>);
                case (BeforeDuringOrAfter.DuringBeforeSave, true):
                case (BeforeDuringOrAfter.DuringSave, true):
                    return typeof(IDuringSaveEventHandlerAsync<>);
                case (BeforeDuringOrAfter.AfterSave, false):
                    return typeof(IAfterSaveEventHandler<>);
                case (BeforeDuringOrAfter.AfterSave, true):
                    return typeof(IAfterSaveEventHandlerAsync<>);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}