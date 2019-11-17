// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using GenericEventRunner.ForEntities;

namespace GenericEventRunner.ForHandlers
{
    public class GenericEventRunnerException : Exception
    {

        public GenericEventRunnerException(string message)
            : base(message)
        {
        }

        public GenericEventRunnerException(string message, EntityEvents callingEntity, IDomainEvent domainEvent)
            : base(message)
        {
            Data.Add("CallingEntityType", callingEntity.GetType().FullName);
            Data.Add("DomainEventType", domainEvent.GetType().FullName);
        }
    }
}