// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayer;
using EntityClasses.DomainEvents;
using GenericEventRunner.ForEntities;
using GenericEventRunner.ForHandlers;

namespace Infrastructure.BeforeEventHandlers
{
    public class OrderCreatedHandler : IBeforeSaveEventHandler<OrderCreatedEvent>
    {
        private readonly ExampleDbContext _context;

        public OrderCreatedHandler(ExampleDbContext context)
        {
            _context = context;
        }

        public void Handle(EntityEvents callingEntity, OrderCreatedEvent domainEvent)
        {
            var taxRateToUse = _context.TaxRates.OrderByDescending(x => x.EffectiveFrom)
                .FirstOrDefault(x => x.EffectiveFrom <= domainEvent.ExpectedDispatchDate);

            if (taxRateToUse == null)
                throw new InvalidOperationException($"There was no take rate valid for the date {domainEvent.ExpectedDispatchDate:yyyy MMMM dd}");

            domainEvent.SetTaxRatePercent(taxRateToUse.TaxRatePercent);
        }
    }
}