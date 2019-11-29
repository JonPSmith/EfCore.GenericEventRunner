// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer;
using EntityClasses.DomainEvents;
using GenericEventRunner.ForEntities;
using GenericEventRunner.ForHandlers;

namespace Infrastructure.AfterEventHandlers
{
    public class OrderReadyToDispatchAfterHandler : IAfterSaveEventHandler<OrderReadyToDispatchEvent>
    {
        public void Handle(EntityEvents callingEntity, OrderReadyToDispatchEvent domainEvent)
        {
            //Send message to dispatch that order has been checked and is ready to go
        }
    }
}