// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using GenericEventRunner.ForEntities;
using Microsoft.Extensions.DependencyInjection;

namespace GenericEventRunner.ForHandlers.Internal
{
    internal class FindRunHandlers
    {
        private readonly IServiceProvider _serviceProvider;

        public FindRunHandlers(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// This finds and runs all the sync BeforeSave handlers built to take this domain event 
        /// </summary>
        /// <param name="entityAndEvent"></param>
        public void DispatchBeforeSave(EntityAndEvent entityAndEvent)
        {
            var eventType = entityAndEvent.DomainEvent.GetType();
            var handlerInterface = typeof(IBeforeSaveEventHandler<>).MakeGenericType(eventType);
            var wrapperType = typeof(BeforeSaveHandler<>).MakeGenericType(eventType);
            var wrappedHandlers = _serviceProvider.GetServices(handlerInterface)
                .Select(handler => (BeforeSaveEventHandler)Activator.CreateInstance(wrapperType, handler)).ToList();

            if (!wrappedHandlers.Any())
                throw new NoEventHandlerFoundException($"Could not find a BeforeSave event handler for the event {eventType.Name}.", 
                    entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);

            foreach (var handler in wrappedHandlers)
            {
                handler.Handle(entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);
            }
        }

        /// <summary>
        /// This finds and runs all the sync and async BeforeSave handlers built to take this domain event 
        /// </summary>
        /// <param name="entityAndEvent"></param>
        public async Task DispatchBeforeSaveAsync(EntityAndEvent entityAndEvent)
        {
            var eventType = entityAndEvent.DomainEvent.GetType();
            var handlerInterface = typeof(IBeforeSaveEventHandler<>).MakeGenericType(eventType);
            var wrapperType = typeof(BeforeSaveHandler<>).MakeGenericType(eventType);
            var wrappedHandlers = _serviceProvider.GetServices(handlerInterface)
                .Select(handler => (BeforeSaveEventHandler)Activator.CreateInstance(wrapperType, handler)).ToList();

            if (!wrappedHandlers.Any())
                throw new NoEventHandlerFoundException($"Could not find a BeforeSave event handler for the event {eventType.Name}.",
                    entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);

            foreach (var handler in wrappedHandlers)
            {
                handler.Handle(entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);
            }
        }

        /// <summary>
        /// This finds and runs all the AfterSave handlers built to take this domain event 
        /// </summary>
        /// <param name="entityAndEvent"></param>
        public void DispatchAfterSave(EntityAndEvent entityAndEvent)
        {
            var eventType = entityAndEvent.DomainEvent.GetType();
            var handlerInterface = typeof(IAfterSaveEventHandler<>).MakeGenericType(eventType);
            var wrapperType = typeof(AfterSaveHandler<>).MakeGenericType(eventType);
            var wrappedHandlers = _serviceProvider.GetServices(handlerInterface)
                .Select(handler => (AfterSaveEventHandler)Activator.CreateInstance(wrapperType, handler)).ToList();

            if (!wrappedHandlers.Any())
                throw new NoEventHandlerFoundException($"Could not find an AfterSave event handler for the event {eventType.Name}.",
                    entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);

            foreach (var handler in wrappedHandlers)
            {
                handler.Handle(entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);
            }
        }

    }
}