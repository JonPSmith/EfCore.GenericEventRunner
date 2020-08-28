// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using GenericEventRunner.DomainParts;
using GenericEventRunner.ForHandlers;
using StatusGeneric;

namespace Test.EventsAndHandlers
{
    public class DuringPreHandlerReturnsErrorStatus : IDuringSaveEventHandler<EventTestDuringPreReturnError>
    {
        public IStatusGeneric Handle(object callingEntity, EventTestDuringPreReturnError domainEvent, Guid uniqueKey)
        {
            ((EntityEventsBase)callingEntity).AddEvent(new EventDoNothing());
            return new StatusGenericHandler().AddError("This is a test");
        }
    }
}