// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer;
using EntityClasses.DomainEvents;
using GenericEventRunner.ForEntities;
using GenericEventRunner.ForHandlers;

namespace Infrastructure.AfterEventHandlers
{
    public class OrderDispatchedAfterHandler : IAfterSaveEventHandler<OrderDispatchedEvent>
    {
        private readonly ExampleDbContext _context;

        public OrderDispatchedAfterHandler(ExampleDbContext context)
        {
            _context = context;
        }

        public void Handle(EntityEvents callingEntity, OrderDispatchedEvent domainEvent)
        {
            //Send final invoice to customer
        }
    }
}