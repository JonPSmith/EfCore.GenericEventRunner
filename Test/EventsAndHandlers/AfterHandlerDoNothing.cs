// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using GenericEventRunner.DomainParts;
using GenericEventRunner.ForHandlers;
using StatusGeneric;

namespace Test.EventsAndHandlers
{
    public class AfterHandlerDoNothing : IAfterSaveEventHandler<EventDoNothing>
    {
        public void Handle(object callingEntity, EventDoNothing domainEvent)
        {
        }
    }
}