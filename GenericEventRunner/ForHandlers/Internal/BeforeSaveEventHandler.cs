// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using GenericEventRunner.ForEntities;

namespace GenericEventRunner.ForHandlers.Internal
{
    internal abstract class BeforeSaveEventHandler
    {
        public abstract void Handle(IDomainEvent domainEvent);
    }

    internal class BeforeSaveHandler<T> : BeforeSaveEventHandler
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
}