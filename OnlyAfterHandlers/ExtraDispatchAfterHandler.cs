// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using EntityClasses.DomainEvents;
using GenericEventRunner.ForHandlers;

namespace OnlyAfterHandlers
{
    public class ExtraDispatchAfterHandler : IAfterSaveEventHandler<OrderReadyToDispatchEvent>
    {
        public void Handle(object callingEntity, OrderReadyToDispatchEvent domainEvent)
        {
            //Send message to dispatch that order has been checked and is ready to go
        }
    }
}