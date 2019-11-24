// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using GenericEventRunner.ForEntities;
using GenericEventRunner.ForHandlers;
using StatusGeneric;

namespace Test.EventsAndHandlers
{
    public class BeforeHandlerReturnsErrorStatus : IBeforeSaveEventHandler<EventTestBeforeReturnError>
    {
        public IStatusGeneric Handle(EntityEvents callingEntity, EventTestBeforeReturnError domainEvent)
        {
            return new StatusGenericHandler().AddError("This is a test");
        }
    }
}