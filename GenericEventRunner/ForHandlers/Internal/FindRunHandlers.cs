// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
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
        /// This finds and runs all the BeforeSave handlers built to take this domain event 
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

        private abstract class BeforeSaveEventHandler
        {
            public abstract void Handle(IDomainEvent domainEvent);
        }

        private class BeforeSaveHandler<T> : BeforeSaveEventHandler
            where T : IDomainEvent
        {
            private readonly IBeforeSaveEventHandler<T> _handler;

            public BeforeSaveHandler(IBeforeSaveEventHandler<T> handler)
            {
                _handler = handler;
            }

            public override void Handle(IDomainEvent domainEvent)
            {
                _handler.Handle((T)domainEvent);
            }
        }

        private abstract class AfterSaveEventHandler
        {
            public abstract void Handle(IDomainEvent domainEvent);
        }

        private class AfterSaveHandler<T> : AfterSaveEventHandler
            where T : IDomainEvent
        {
            private readonly IAfterSaveEventHandler<T> _handler;

            public AfterSaveHandler(IAfterSaveEventHandler<T> handler)
            {
                _handler = handler;
            }

            public override void Handle(IDomainEvent domainEvent)
            {
                _handler.Handle((T)domainEvent);
            }
        }
    }
}