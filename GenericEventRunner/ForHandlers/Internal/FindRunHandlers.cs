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
        /// <param name="domainEvent"></param>
        public void DispatchBeforeSave(IDomainEvent domainEvent)
        {
            var handlerInterface = typeof(IBeforeSaveEventHandler<>).MakeGenericType(domainEvent.GetType());
            var wrapperType = typeof(BeforeSaveHandler<>).MakeGenericType(domainEvent.GetType());
            var wrappedHandlers = _serviceProvider.GetServices(handlerInterface)
                .Select(handler => (BeforeSaveEventHandler)Activator.CreateInstance(wrapperType, handler));

            foreach (var handler in wrappedHandlers)
            {
                handler.Handle(domainEvent);
            }
        }

        /// <summary>
        /// This finds and runs all the sync and async BeforeSave handlers built to take this domain event 
        /// </summary>
        /// <param name="domainEvent"></param>
        public async Task DispatchBeforeSaveAsync(IDomainEvent domainEvent)
        {
            var handlerInterface = typeof(IBeforeSaveEventHandler<>).MakeGenericType(domainEvent.GetType());
            var wrapperType = typeof(BeforeSaveHandler<>).MakeGenericType(domainEvent.GetType());
            var wrappedHandlers = _serviceProvider.GetServices(handlerInterface)
                .Select(handler => (BeforeSaveEventHandler)Activator.CreateInstance(wrapperType, handler));

            foreach (var handler in wrappedHandlers)
            {
                handler.Handle(domainEvent);
            }
        }

        /// <summary>
        /// This finds and runs all the AfterSave handlers built to take this domain event 
        /// </summary>
        /// <param name="domainEvent"></param>
        public void DispatchAfterSave(IDomainEvent domainEvent)
        {
            var handlerInterface = typeof(IAfterSaveEventHandler<>).MakeGenericType(domainEvent.GetType());
            var wrapperType = typeof(AfterSaveHandler<>).MakeGenericType(domainEvent.GetType());
            var wrappedHandlers = _serviceProvider.GetServices(handlerInterface)
                .Select(handler => (AfterSaveEventHandler)Activator.CreateInstance(wrapperType, handler));

            foreach (var handler in wrappedHandlers)
            {
                handler.Handle(domainEvent);
            }
        }

    }
}