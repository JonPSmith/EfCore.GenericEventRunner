// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using EntityClasses.DomainEvents;
using GenericEventRunner.ForHandlers;
using StatusGeneric;

namespace OnlyBeforeHandlers
{
    public class BeforeHandler : IBeforeSaveEventHandler<OrderCreatedEvent>
    {

        public IStatusGeneric Handle(object callingEntity, OrderCreatedEvent domainEvent)
        {
            return null;
        }
    }
}