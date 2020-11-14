// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using GenericEventRunner.DomainParts;

namespace EntityClasses.DomainEvents
{
    [RemoveDuplicateEvents]
    public class DeDupEvent : IEntityEvent
    {
        public DeDupEvent(Action actionToCall)
        {
            ActionToCall = actionToCall;
        }

        public Action ActionToCall { get; }
    }
}