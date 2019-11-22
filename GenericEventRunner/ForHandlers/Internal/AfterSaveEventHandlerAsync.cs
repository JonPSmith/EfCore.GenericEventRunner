// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading.Tasks;
using GenericEventRunner.ForEntities;

namespace GenericEventRunner.ForHandlers.Internal
{
    internal abstract class AfterSaveEventHandlerAsync
    {
        public abstract Task HandleAsync(EntityEvents callingEntity, IDomainEvent domainEvent);
    }

    internal class AfterSaveHandlerAsync<T> : AfterSaveEventHandlerAsync
        where T : IDomainEvent
    {
        private readonly IAfterSaveEventHandlerAsync<T> _handler;

        public AfterSaveHandlerAsync(IAfterSaveEventHandlerAsync<T> handler)
        {
            _handler = handler;
        }

        public override async Task HandleAsync(EntityEvents callingEntity, IDomainEvent domainEvent)
        {
            await _handler.HandleAsync(callingEntity, (T)domainEvent).ConfigureAwait(false);
        }
    }
}