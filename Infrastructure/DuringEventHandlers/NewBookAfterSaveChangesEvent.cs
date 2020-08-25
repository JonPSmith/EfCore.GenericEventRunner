// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using EntityClasses.DomainEvents;
using GenericEventRunner.ForHandlers;
using Microsoft.Extensions.Logging;
using StatusGeneric;

namespace Infrastructure.DuringEventHandlers
{
    public class NewBookAfterSaveChangesEvent : IDuringSaveEventHandler<NewBookEvent>
    {
        private readonly ILogger<EventsRunner> _logger;

        public NewBookAfterSaveChangesEvent(ILogger<EventsRunner> logger)
        {
            _logger = logger;
        }

        public IStatusGeneric Handle(object callingEntity, NewBookEvent domainEvent, Guid uniqueKey)
        {
            _logger.LogInformation($"Log from NewBookAfterSaveChangesEvent. Unique value = {uniqueKey}");
            return new StatusGenericHandler();
        }
    }
}