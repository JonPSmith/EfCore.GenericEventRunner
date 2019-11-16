// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;

namespace EntityClasses
{
    public class TaxRate
    {
        public TaxRate(DateTime effectiveFrom, decimal taxRatePercent)
        {
            EffectiveFrom = effectiveFrom;
            TaxRatePercent = taxRatePercent;
        }

        public int TaxRateId { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public decimal TaxRatePercent { get; set; }
    }
}