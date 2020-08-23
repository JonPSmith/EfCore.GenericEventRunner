// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading.Tasks;
using GenericEventRunner.DomainParts;

namespace GenericEventRunner.ForHandlers.Internal
{
    internal abstract class AfterSaveEventHandlerAsync
    {
        public abstract Task HandleAsync(object callingEntity, IDomainEvent domainEvent);
    }

    internal class AfterSaveHandlerAsync<T> : AfterSaveEventHandlerAsync
        where T : IDomainEvent
    {
        private readonly IAfterSaveEventHandlerAsync<T> _handler;

        public AfterSaveHandlerAsync(IAfterSaveEventHandlerAsync<T> handler)
        {
            _handler = handler;
        }

        public override Task HandleAsync(object callingEntity, IDomainEvent domainEvent)
        {
            return _handler.HandleAsync(callingEntity, (T)domainEvent);
        }
    }
}