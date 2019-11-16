// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayer;

namespace Infrastructure.BeforeEventHandlers.Internal
{
    public class TaxRateLookup
    {
        private readonly ExampleDbContext _context;

        public TaxRateLookup(ExampleDbContext context)
        {
            _context = context;
        }

        public decimal GetTaxRateInEffect(DateTime expectedDispatchDate)
        {
            var taxRateToUse = _context.TaxRates.OrderByDescending(x => x.EffectiveFrom)
                .FirstOrDefault(x => x.EffectiveFrom <= expectedDispatchDate);

            if (taxRateToUse == null)
                throw new InvalidOperationException($"There was no take rate valid for the date {expectedDispatchDate:yyyy MMMM dd}");

            return taxRateToUse.TaxRatePercent;
        }
    }
}