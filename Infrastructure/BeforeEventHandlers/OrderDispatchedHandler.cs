// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer;
using EntityClasses;
using EntityClasses.DomainEvents;
using GenericEventRunner.ForEntities;
using GenericEventRunner.ForHandlers;
using Infrastructure.BeforeEventHandlers.Internal;

namespace Infrastructure.BeforeEventHandlers
{
    public class OrderDispatchedHandler : IBeforeSaveEventHandler<OrderDispatchedEvent>
    {
        private readonly ExampleDbContext _context;
        private readonly TaxRateLookup _rateFinder;

        public OrderDispatchedHandler(ExampleDbContext context)
        {
            _context = context;
            _rateFinder = new TaxRateLookup(context);
        }

        public void Handle(EntityEvents callingEntity, OrderDispatchedEvent domainEvent)
        {
            //Update the rate as the date may have changed
            domainEvent.SetTaxRatePercent(_rateFinder.GetTaxRateInEffect(domainEvent.ActualDispatchDate));

            var orderId = ((Order) callingEntity).OrderId;
            foreach (var lineItem in _context.LineItems.Where(x => x.OrderId == orderId))
            {
                var stock = _context.Find<ProductStock>(lineItem.ProductCode);
                stock.NumAllocated -= lineItem.NumOrdered;
                stock.NumInStock -= lineItem.NumOrdered;
            }
        }
    }
}