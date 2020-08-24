// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using GenericEventRunner.DomainParts;
using StatusGeneric;

namespace GenericEventRunner.ForHandlers.Internal
{
    internal abstract class DuringSaveEventHandler
    {
        public abstract IStatusGeneric Handle(object callingEntity, IDomainEvent domainEvent, Guid uniqueKey);
    }

    internal class DuringSaveHandler<T> : DuringSaveEventHandler
        where T : IDomainEvent
    {
        private readonly IDuringSaveEventHandler<T> _handler;

        public DuringSaveHandler(IDuringSaveEventHandler<T> handler)
        {
            _handler = handler;
        }

        public override IStatusGeneric Handle(object callingEntity, IDomainEvent domainEvent, Guid uniqueKey)
        {
            return _handler.Handle(callingEntity, (T)domainEvent, uniqueKey);
        }
    }
}