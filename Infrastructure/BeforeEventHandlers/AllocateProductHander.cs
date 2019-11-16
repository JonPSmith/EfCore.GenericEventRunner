// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using DataLayer;
using EntityClasses;
using EntityClasses.DomainEvents;
using GenericEventRunner.ForEntities;
using GenericEventRunner.ForHandlers;

namespace Infrastructure.BeforeEventHandlers
{
    public class AllocateProductHandler : IBeforeSaveEventHandler<AllocateProductEvent>
    {
        private readonly ExampleDbContext _context;

        public AllocateProductHandler(ExampleDbContext context)
        {
            _context = context;
        }

        public void Handle(EntityEvents callingEntity, AllocateProductEvent domainEvent)
        {
            var stock = _context.Find<ProductStock>(domainEvent.ProductCode);
            if (stock == null)
                throw new InvalidOperationException($"could not find the stock for product code {domainEvent.ProductCode} ");

            stock.NumAllocated += domainEvent.NumOrdered;
        }
    }
}