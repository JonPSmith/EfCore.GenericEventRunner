// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using GenericEventRunner.ForEntities;

namespace EntityClasses
{
    public class ProductStock 
    {
        public ProductStock(Guid productCode, int numInStock)
        {
            ProductCode = productCode;
            NumInStock = numInStock;
            NumAllocated = 0;
        }

        public Guid ProductCode { get; set; }

        public int NumInStock { get; set; }

        public int NumAllocated { get; set; }
    }
}