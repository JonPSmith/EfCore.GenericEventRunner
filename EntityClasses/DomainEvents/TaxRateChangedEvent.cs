// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using GenericEventRunner.DomainParts;

namespace EntityClasses.DomainEvents
{
    public class TaxRateChangedEvent : IDomainEvent
    {
        public TaxRateChangedEvent(decimal newTaxRate, Action refreshGrandTotalPrice)
        {
            NewTaxRate = newTaxRate;
            RefreshGrandTotalPrice = refreshGrandTotalPrice;
        }

        public decimal NewTaxRate { get; }

        public Action RefreshGrandTotalPrice { get; }
    }
}