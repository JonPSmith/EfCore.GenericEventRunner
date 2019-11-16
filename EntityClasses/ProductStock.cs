// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using GenericEventRunner.ForEntities;

namespace EntityClasses
{
    public class ProductStock : EntityEvents
    {
        public ProductStock(Guid productCode, int numInStock)
        {
            ProductCode = productCode;
            NumInStock = numInStock;
            NumAllocated = 0;
        }

        public Guid ProductCode { get; private set; }

        public int NumInStock { get; private set; }

        public int NumAllocated { get; private set; }
    }
}