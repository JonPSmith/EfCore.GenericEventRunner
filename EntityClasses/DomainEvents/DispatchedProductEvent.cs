// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using GenericEventRunner.ForEntities;

namespace EntityClasses.DomainEvents
{
    public class DispatchedProductEvent : IDomainEvent
    {
        public DispatchedProductEvent(Guid productCode, int numOrdered)
        {
            ProductCode = productCode;
            NumOrdered = numOrdered;
        }

        public Guid ProductCode { get; }
        public int NumOrdered { get; }
    }
}