// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;

namespace EntityClasses
{
    public class LineItem
    {
        internal LineItem(int lineNum, string productName, decimal productPrice, int numOrdered)
        {
            LineNum = lineNum;
            ProductName = productName;
            ProductPrice = productPrice;
            NumOrdered = numOrdered;
        }


        public int LineItemId { get; private set; }

        public int LineNum { get; private set; }

        public string ProductName { get; private set; }

        public decimal ProductPrice { get; private set; }

        public int NumOrdered { get; private set; }

        //------------------------------------------
        //relationships

        public int OrderId { get; private set; }
    }
}