// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using GenericEventRunner.ForEntities;

namespace GenericEventRunner.ForHandlers.Internal
{
    internal abstract class AfterSaveEventHandler
    {
        public abstract void Handle(EntityEvents callingEntity, IDomainEvent domainEvent);
    }

    internal class AfterSaveHandler<T> : AfterSaveEventHandler
        where T : IDomainEvent
    {
        private readonly IAfterSaveEventHandler<T> _handler;

        public AfterSaveHandler(IAfterSaveEventHandler<T> handler)
        {
            _handler = handler;
        }

        public override void Handle(EntityEvents callingEntity, IDomainEvent domainEvent)
        {
            _handler.Handle(callingEntity, (T)domainEvent);
        }
    }
}