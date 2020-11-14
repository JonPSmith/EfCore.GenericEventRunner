// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using EntityClasses.DomainEvents;
using GenericEventRunner.ForHandlers;
using StatusGeneric;

namespace Infrastructure.BeforeEventHandlers
{
    public class DeDupBeforeEventHandler : IBeforeSaveEventHandler<DeDupEvent>
    {
        public IStatusGeneric Handle(object callingEntity, DeDupEvent domainEvent)
        {
            domainEvent.ActionToCall();
            return null;
        }
    }
}