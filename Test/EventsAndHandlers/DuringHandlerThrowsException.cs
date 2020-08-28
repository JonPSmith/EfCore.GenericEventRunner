// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using GenericEventRunner.ForHandlers;
using StatusGeneric;

namespace Test.EventsAndHandlers
{
    public class DuringHandlerThrowsException : IDuringSaveEventHandler<EventTestDuringExceptionHandler>
    {
        public IStatusGeneric Handle(object callingEntity, EventTestDuringExceptionHandler domainEvent, Guid uniqueKey)
        {
            throw new ApplicationException(nameof(DuringHandlerThrowsException));
        }
    }
}