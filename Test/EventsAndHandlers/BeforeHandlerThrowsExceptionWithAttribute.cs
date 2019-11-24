// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using GenericEventRunner.ForEntities;
using GenericEventRunner.ForHandlers;
using Microsoft.Extensions.DependencyInjection;
using StatusGeneric;

namespace Test.EventsAndHandlers
{
    [EventHandlerConfig(ServiceLifetime.Scoped)]
    public class BeforeHandlerThrowsExceptionWithAttribute : IBeforeSaveEventHandler<EventTestExceptionHandlerWithAttribute>
    {
        public IStatusGeneric Handle(EntityEvents callingEntity, EventTestExceptionHandlerWithAttribute domainEvent)
        {
            throw new ApplicationException(nameof(BeforeHandlerThrowsExceptionWithAttribute));
        }
    }
}