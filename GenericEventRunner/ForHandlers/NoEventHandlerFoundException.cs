// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using GenericEventRunner.ForEntities;

namespace GenericEventRunner.ForHandlers
{
    public class NoEventHandlerFoundException : Exception
    {
        public EntityEvents CallingEntity { get; } 
        public IDomainEvent DomainEvent { get; }

        public NoEventHandlerFoundException(string message, EntityEvents callingEntity, IDomainEvent domainEvent)
            : base(message)
        {
            CallingEntity = callingEntity;
            DomainEvent = domainEvent;
        }
    }
}