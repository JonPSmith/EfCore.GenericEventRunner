// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using GenericEventRunner.DomainParts;

namespace GenericEventRunner.ForHandlers.Internal
{
    internal abstract class AfterSaveEventHandlerAsync
    {
        public abstract void Handle(EntityEventsBase callingEntity, IDomainEvent domainEvent);
    }

    internal class AfterSaveHandlerAsync<T> : AfterSaveEventHandlerAsync
        where T : IDomainEvent
    {
        private readonly IAfterSaveEventHandlerAsync<T> _handler;

        public AfterSaveHandlerAsync(IAfterSaveEventHandlerAsync<T> handler)
        {
            _handler = handler;
        }

        public override void Handle(EntityEventsBase callingEntity, IDomainEvent domainEvent)
        {
            _handler.HandleAsync(callingEntity, (T)domainEvent);
        }
    }
}