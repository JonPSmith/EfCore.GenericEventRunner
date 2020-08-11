// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using GenericEventRunner.DomainParts;

namespace EntityClasses.DomainEvents
{
    public class OrderCreatedEvent : IDomainEvent
    {
        public OrderCreatedEvent(DateTime expectedDispatchDate, Action<decimal> setTaxRatePercent)
        {
            ExpectedDispatchDate = expectedDispatchDate;
            SetTaxRatePercent = setTaxRatePercent;
        }

        public DateTime ExpectedDispatchDate { get; }

        public Action<decimal> SetTaxRatePercent { get; }
    }
}