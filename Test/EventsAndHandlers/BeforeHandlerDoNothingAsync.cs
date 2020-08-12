// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading.Tasks;
using GenericEventRunner.DomainParts;
using GenericEventRunner.ForHandlers;
using StatusGeneric;

namespace Test.EventsAndHandlers
{
    public class BeforeHandlerDoNothingAsync : IBeforeSaveEventHandlerAsync<EventDoNothing>
    {
        public Task<IStatusGeneric> HandleAsync(EntityEventsBase callingEntity, EventDoNothing domainEvent)
        {
            return Task.FromResult<IStatusGeneric>(null);
        }
    }
}