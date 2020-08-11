// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using GenericEventRunner.DomainParts;

namespace EntityClasses.DomainEvents
{
    public class AllocateProductEvent : IDomainEvent
    {
        public AllocateProductEvent(string productName, int numOrdered)
        {
            ProductName = productName;
            NumOrdered = numOrdered;
        }

        public string ProductName { get; }
        public int NumOrdered { get; }
    }
}