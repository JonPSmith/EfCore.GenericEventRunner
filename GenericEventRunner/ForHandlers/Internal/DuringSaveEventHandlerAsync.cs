// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using GenericEventRunner.DomainParts;
using StatusGeneric;

namespace GenericEventRunner.ForHandlers.Internal
{
    internal abstract class DuringSaveEventHandlerAsync
    {
        public abstract Task<IStatusGeneric> HandleAsync(object callingEntity, IDomainEvent domainEvent, Guid uniqueKey);
    }

    internal class DuringSaveHandlerAsync<T> : DuringSaveEventHandlerAsync
        where T : IDomainEvent
    {
        private readonly IDuringSaveEventHandlerAsync<T> _handler;

        public DuringSaveHandlerAsync(IDuringSaveEventHandlerAsync<T> handler)
        {
            _handler = handler;
        }

        public override Task<IStatusGeneric> HandleAsync(object callingEntity, IDomainEvent domainEvent, Guid uniqueKey)
        {
            return _handler.HandleAsync(callingEntity, (T)domainEvent, uniqueKey);
        }
    }
}