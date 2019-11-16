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
            var handlerInterface = typeof(IBeforeSaveEventHandler<>).MakeGenericType(entityAndEvent.DomainEvent.GetType());
            var wrapperType = typeof(BeforeSaveHandler<>).MakeGenericType(entityAndEvent.DomainEvent.GetType());
            var wrappedHandlers = _serviceProvider.GetServices(handlerInterface)
                .Select(handler => (BeforeSaveEventHandler)Activator.CreateInstance(wrapperType, handler));

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
            var handlerInterface = typeof(IBeforeSaveEventHandler<>).MakeGenericType(entityAndEvent.DomainEvent.GetType());
            var wrapperType = typeof(BeforeSaveHandler<>).MakeGenericType(entityAndEvent.DomainEvent.GetType());
            var wrappedHandlers = _serviceProvider.GetServices(handlerInterface)
                .Select(handler => (BeforeSaveEventHandler)Activator.CreateInstance(wrapperType, handler));

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
            var handlerInterface = typeof(IAfterSaveEventHandler<>).MakeGenericType(entityAndEvent.DomainEvent.GetType());
            var wrapperType = typeof(AfterSaveHandler<>).MakeGenericType(entityAndEvent.DomainEvent.GetType());
            var wrappedHandlers = _serviceProvider.GetServices(handlerInterface)
                .Select(handler => (AfterSaveEventHandler)Activator.CreateInstance(wrapperType, handler));

            foreach (var handler in wrappedHandlers)
            {
                handler.Handle(entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);
            }
        }

    }
}