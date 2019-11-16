// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using DataLayer;
using EntityClasses;

namespace Test.EfHelpers
{
    public static class SeedExtensions
    {
        public static List<ProductStock> SeedExampleProductStock(this ExampleDbContext context)
        {
            var prodStocks = new List<ProductStock>
            {
                new ProductStock(Guid.NewGuid(), 5),
                new ProductStock(Guid.NewGuid(), 10),
                new ProductStock(Guid.NewGuid(), 20),
            };
            context.AddRange(prodStocks);
            context.SaveChanges();
            return prodStocks;
        }
    }
}