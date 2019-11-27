// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using EntityClasses.DomainEvents;
using GenericEventRunner.ForEntities;
using GenericEventRunner.ForHandlers;
using StatusGeneric;

namespace Infrastructure.BeforeEventHandlers
{
    public class TaxRateChangedHandler : IBeforeSaveEventHandler<TaxRateChangedEvent>
    {
        public IStatusGeneric Handle(EntityEvents callingEntity, TaxRateChangedEvent domainEvent)
        {
            //do something with the new tax rate

            domainEvent.RefreshGrandTotalPrice();  //example of using Action/Func to set something inside the calling class

            return new StatusGenericHandler();
        }
    }
}