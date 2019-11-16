// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using GenericEventRunner.ForEntities;

namespace GenericEventRunner.ForHandlers.Internal
{
    internal abstract class BeforeSaveEventHandler
    {
        public abstract void Handle(EntityEvents callingEntity, IDomainEvent domainEvent);
    }

    internal class BeforeSaveHandler<T> : BeforeSaveEventHandler
        where T : IDomainEvent
    {
        private readonly IBeforeSaveEventHandler<T> _handler;

        public BeforeSaveHandler(EntityEvents callingEntity, IBeforeSaveEventHandler<T> handler)
        {
            _handler = handler;
        }

        public override void Handle(EntityEvents callingEntity, IDomainEvent domainEvent)
        {
            _handler.Handle(callingEntity, (T)domainEvent);
        }
    }
}