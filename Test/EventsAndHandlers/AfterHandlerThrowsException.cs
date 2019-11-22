// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using GenericEventRunner.ForEntities;
using GenericEventRunner.ForHandlers;

namespace Test.EventsAndHandlers
{
    public class AfterHandlerThrowsException : IAfterSaveEventHandler<EventTestAfterExceptionHandler>
    {
        public void Handle(EntityEvents callingEntity, EventTestAfterExceptionHandler domainEvent)
        {
            throw new ApplicationException(nameof(BeforeHandlerThrowsException));
        }
    }
}