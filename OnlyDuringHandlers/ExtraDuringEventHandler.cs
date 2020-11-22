// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using EntityClasses.DomainEvents;
using GenericEventRunner.ForHandlers;
using StatusGeneric;

namespace OnlyDuringHandlers
{
    public class ExtraDuringEventHandler : IDuringSaveEventHandler<NewBookEvent>
    {
        public IStatusGeneric Handle(object callingEntity, NewBookEvent domainEvent, Guid uniqueKey)
        {
            return new StatusGenericHandler();
        }
    }
}