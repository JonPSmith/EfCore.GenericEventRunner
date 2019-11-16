// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;

namespace EntityClasses.SupportClasses
{
    public class BasketItemDto
    {
        public Guid ProductCode { get; set; }

        public decimal ProductPrice { get; set; }
        public int NumOrdered { get; set; }
    }
}