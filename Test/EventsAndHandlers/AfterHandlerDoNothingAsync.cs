// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading.Tasks;
using GenericEventRunner.DomainParts;
using GenericEventRunner.ForHandlers;

namespace Test.EventsAndHandlers
{
    public class AfterHandlerDoNothingAsync : IAfterSaveEventHandlerAsync<EventDoNothing>
    {

        public Task HandleAsync(EntityEventsBase callingEntity, EventDoNothing domainEvent)
        {
            return Task.CompletedTask;
        }
    }
}