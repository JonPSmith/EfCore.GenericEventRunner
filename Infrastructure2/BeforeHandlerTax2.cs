using System;
using EntityClasses.DomainEvents;
using GenericEventRunner.ForHandlers;
using StatusGeneric;

namespace Infrastructure2
{
    public class BeforeHandlerTax2 : IBeforeSaveEventHandler<TaxRateChangedEvent>
    {
        public IStatusGeneric Handle(object callingEntity, TaxRateChangedEvent domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
