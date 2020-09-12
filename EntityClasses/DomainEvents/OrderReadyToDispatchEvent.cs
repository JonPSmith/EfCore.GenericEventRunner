// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using GenericEventRunner.DomainParts;

namespace EntityClasses.DomainEvents
{
    public class OrderReadyToDispatchEvent : IEntityEvent
    {
        public OrderReadyToDispatchEvent(DateTime actualDispatchDate, Action<decimal> setTaxRatePercent)
        {
            ActualDispatchDate = actualDispatchDate;
            SetTaxRatePercent = setTaxRatePercent;
        }

        public DateTime ActualDispatchDate { get; }

        public Action<decimal> SetTaxRatePercent { get; }
    }
}