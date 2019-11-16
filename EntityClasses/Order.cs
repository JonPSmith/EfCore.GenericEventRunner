// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using EntityClasses.DomainEvents;
using EntityClasses.SupportClasses;
using GenericEventRunner.ForEntities;

namespace EntityClasses
{
    public class Order : EntityEvents
    {
        private HashSet<LineItem> _LineItems;

        public int OrderId { get; private set; }
        public string UserId { get; private set; }

        //The date we expect to dispatch the order
        public DateTime DispatchDate { get; private set; }

        //Price and tax 
        public double TotalPriceNoTax { get; private set; }
        public double TaxRatePercent { get; private set; }
        public double GrandTotalPrice { get; private set; } //= TotalPriceNoTax * (1 + TaxRatePercent/100) 

        //----------------------------------------------
        //Relationships 

        public IEnumerable<LineItem> LineItems => _LineItems.ToList();

        private Order() { } //For EF Core

        public Order(string userId, DateTime dispatchDate, ICollection<BasketItemDto> orderLines)
        {
            UserId = userId;
            DispatchDate = dispatchDate;

            var lineNum = 1;
            _LineItems = new HashSet<LineItem>(orderLines
                .Select(x => new LineItem(lineNum++, x.ProductCode, x.ProductPrice, x.NumOrdered)));

            TotalPriceNoTax = 0;
            foreach (var basketItem in orderLines)
            {
                TotalPriceNoTax += basketItem.ProductPrice * basketItem.NumOrdered;
                AddEvent(new AllocateProductEvent(basketItem.ProductCode, basketItem.NumOrdered));
            }
        }

        public void OrderHasBeenDispatched(DateTime newDispatchDate)
        {
            if (_LineItems == null)
                throw new ApplicationException("This method requires the LineItems to be included.");

            DispatchDate = newDispatchDate;
            foreach (var lineItem in _LineItems)
            {
                AddEvent(new DispatchedProductEvent(lineItem.ProductCode, lineItem.NumOrdered));
            }
        }

        //--------------------------------------------------
        //private methods

    }
}