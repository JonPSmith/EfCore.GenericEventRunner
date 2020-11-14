// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using EntityClasses.DomainEvents;
using GenericEventRunner.ForHandlers;

namespace Infrastructure.AfterEventHandlers
{
    public class DeDupAfterEventHandler : IAfterSaveEventHandler<DeDupEvent>
    {
        public void Handle(object callingEntity, DeDupEvent domainEvent)
        {
            domainEvent.ActionToCall();
        }
    }
}