// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using EntityClasses.DomainEvents;
using GenericEventRunner.ForHandlers;
using Microsoft.Extensions.Logging;
using StatusGeneric;

namespace Infrastructure.DuringEventHandlers
{
    public class NewBookDuringEventHandlerAsync : IDuringSaveEventHandlerAsync<NewBookEvent>
    {
        private readonly ILogger<EventsRunner> _logger;

        public NewBookDuringEventHandlerAsync(ILogger<EventsRunner> logger)
        {
            _logger = logger;
        }

        public async Task<IStatusGeneric> HandleAsync(object callingEntity, NewBookEvent domainEvent, Guid uniqueKey)
        {
            _logger.LogInformation($"Log from {GetType().Name}. Unique value = {uniqueKey}");
            return new StatusGenericHandler();
        }
    }
}