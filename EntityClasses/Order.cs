﻿// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using EntityClasses.DomainEvents;
using EntityClasses.SupportClasses;
using GenericEventRunner.DomainParts;

namespace EntityClasses
{
    public class Order : EntityEventsBase
    {
        private HashSet<LineItem> _LineItems;

        public int OrderId { get; private set; }
        public string UserId { get; private set; }

        //The date we expect to dispatch the order
        public DateTime DispatchDate { get; private set; }
        public decimal TotalPriceNoTax { get; private set; }

        //Price and tax 
        private decimal _taxRatePercent;
        public decimal TaxRatePercent
        {
            get => _taxRatePercent;
            private set
            {
                if (value != _taxRatePercent)
                    AddEvent(new TaxRateChangedEvent(value, RefreshGrandTotalPrice));
                _taxRatePercent = value;
            }
        }

        private void RefreshGrandTotalPrice()
        {
            GrandTotalPrice = TotalPriceNoTax * (1 + TaxRatePercent / 100);
        }

        private void SetTaxRatePercent(decimal newValue)
        {
            TaxRatePercent = newValue;
        }

        public decimal GrandTotalPrice { get; private set; } // should be set by RefreshGrandTotalPrice method

        //----------------------------------------------
        //Relationships 

        public IEnumerable<LineItem> LineItems => _LineItems.ToList();

        private Order() { } //For EF Core

        public Order(string userId, DateTime expectedDispatchDate, ICollection<BasketItemDto> orderLines)
        {
            UserId = userId;
            DispatchDate = expectedDispatchDate;
            AddEvent(new OrderCreatedEvent(expectedDispatchDate, SetTaxRatePercent));

            var lineNum = 1;
            _LineItems = new HashSet<LineItem>(orderLines
                .Select(x => new LineItem(lineNum++, x.ProductName, x.ProductPrice, x.NumOrdered)));

            TotalPriceNoTax = 0;
            foreach (var basketItem in orderLines)
            {
                TotalPriceNoTax += basketItem.ProductPrice * basketItem.NumOrdered;
                AddEvent(new AllocateProductEvent(basketItem.ProductName, basketItem.NumOrdered));
            }
        }

        public void OrderReadyForDispatch(DateTime newDispatchDate)
        {
            if (OrderId == 0)
                throw new InvalidOperationException("You cannot call this method until the Order is written to the database.");

            DispatchDate = newDispatchDate;
            AddEvent(new OrderReadyToDispatchEvent(DispatchDate, SetTaxRatePercent), EventToSend.BeforeAndAfterSave);
        }

    }
}