// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer;
using EntityClasses;
using EntityClasses.DomainEvents;
using GenericEventRunner.DomainParts;
using GenericEventRunner.ForHandlers;
using Infrastructure.BeforeEventHandlers.Internal;
using StatusGeneric;

namespace Infrastructure.BeforeEventHandlers
{
    public class OrderDispatchedBeforeHandler : IBeforeSaveEventHandler<OrderReadyToDispatchEvent>
    {
        private readonly ExampleDbContext _context;
        private readonly TaxRateLookup _rateFinder;

        public OrderDispatchedBeforeHandler(ExampleDbContext context)
        {
            _context = context;
            _rateFinder = new TaxRateLookup(context);
        }

        public IStatusGeneric Handle(object callingEntity, OrderReadyToDispatchEvent domainEvent)
        {
            var status = new StatusGenericHandler();
            //Update the rate as the date may have changed
            domainEvent.SetTaxRatePercent(_rateFinder.GetTaxRateInEffect(domainEvent.ActualDispatchDate));

            var orderId = ((Order) callingEntity).OrderId;
            foreach (var lineItem in _context.LineItems.Where(x => x.OrderId == orderId))
            {
                var stock = _context.Find<ProductStock>(lineItem.ProductName);
                if (stock.NumInStock < lineItem.NumOrdered)
                    return status.AddError(
                        $"I could not dispatch this order because there wasn't enough {lineItem.ProductName} in stock.");
                stock.NumAllocated -= lineItem.NumOrdered;
                stock.NumInStock -= lineItem.NumOrdered;
            }

            return status;
        }
    }
}