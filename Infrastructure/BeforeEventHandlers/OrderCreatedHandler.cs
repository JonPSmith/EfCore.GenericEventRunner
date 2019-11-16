// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayer;
using EntityClasses.DomainEvents;
using GenericEventRunner.ForEntities;
using GenericEventRunner.ForHandlers;
using Infrastructure.BeforeEventHandlers.Internal;

namespace Infrastructure.BeforeEventHandlers
{
    public class OrderCreatedHandler : IBeforeSaveEventHandler<OrderCreatedEvent>
    {
        private readonly TaxRateLookup _rateFinder;

        public OrderCreatedHandler(ExampleDbContext context)
        {
            _rateFinder = new TaxRateLookup(context);
        }

        public void Handle(EntityEvents callingEntity, OrderCreatedEvent domainEvent)
        {
            domainEvent.SetTaxRatePercent(_rateFinder.GetTaxRateInEffect(domainEvent.ExpectedDispatchDate));
        }
    }
}