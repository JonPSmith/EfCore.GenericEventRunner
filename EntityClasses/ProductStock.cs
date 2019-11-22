// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace EntityClasses
{
    public class ProductStock 
    {
        public ProductStock(string productName, int numInStock)
        {
            ProductName = productName;
            NumInStock = numInStock;
            NumAllocated = 0;
        }

        public string ProductName { get; set; }

        public int NumInStock { get; set; }

        public int NumAllocated { get; set; }
    }
}