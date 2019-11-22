// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using GenericEventRunner.ForEntities;
using GenericEventRunner.ForHandlers;
using StatusGeneric;

namespace Test.TestEventHandlers
{
    public class BeforeHandlerCircularEvent : IBeforeSaveEventHandler<EventCircularEvent>
    {
        public IStatusGeneric Handle(EntityEvents callingEntity, EventCircularEvent domainEvent)
        {
            callingEntity.AddEvent(domainEvent);
            return null;
        }
    }
}