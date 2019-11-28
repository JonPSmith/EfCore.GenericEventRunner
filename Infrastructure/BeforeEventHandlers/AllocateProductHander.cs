// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using DataLayer;
using EntityClasses;
using EntityClasses.DomainEvents;
using GenericEventRunner.ForEntities;
using GenericEventRunner.ForHandlers;
using StatusGeneric;

namespace Infrastructure.BeforeEventHandlers
{
    public class AllocateProductHandler : IBeforeSaveEventHandler<AllocateProductEvent>
    {
        private readonly ExampleDbContext _context;

        public AllocateProductHandler(ExampleDbContext context)
        {
            _context = context;
        }

        public IStatusGeneric Handle(EntityEvents callingEntity, AllocateProductEvent domainEvent)
        {
            var status = new StatusGenericHandler();
            var stock = _context.Find<ProductStock>(domainEvent.ProductName);
            if (stock == null)
                throw new ApplicationException($"could not find the stock for product called {domainEvent.ProductName} ");

            if (stock.NumInStock < domainEvent.NumOrdered)
                return status.AddError(
                    $"I could not accept this order because there wasn't enough {domainEvent.ProductName} in stock.");

            stock.NumAllocated += domainEvent.NumOrdered;
            return status;
        }
    }
}