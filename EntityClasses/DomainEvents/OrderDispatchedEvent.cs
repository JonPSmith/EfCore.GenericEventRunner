// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using GenericEventRunner.ForEntities;

namespace EntityClasses.DomainEvents
{
    public class OrderDispatchedEvent : IDomainEvent
    {
        public OrderDispatchedEvent(int orderId)
        {
            OrderId = orderId;
        }

        public int OrderId { get; }
    }
}