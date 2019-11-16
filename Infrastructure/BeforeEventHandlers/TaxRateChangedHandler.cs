// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using EntityClasses.DomainEvents;
using GenericEventRunner.ForEntities;
using GenericEventRunner.ForHandlers;

namespace Infrastructure.BeforeEventHandlers
{
    public class TaxRateChangedHandler : IBeforeSaveEventHandler<TaxRateChangedEvent>
    {
        public void Handle(EntityEvents callingEntity, TaxRateChangedEvent domainEvent)
        {
            domainEvent.RefreshGrandTotalPrice();
        }
    }
}