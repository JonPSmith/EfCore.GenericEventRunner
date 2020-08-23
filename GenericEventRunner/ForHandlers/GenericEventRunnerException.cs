// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using GenericEventRunner.DomainParts;

namespace GenericEventRunner.ForHandlers
{
    /// <summary>
    /// This is used to report any problems in the GenericEventRunner
    /// </summary>
    public class GenericEventRunnerException : Exception
    {
        /// <summary>
        /// This creates an exception just with a message
        /// </summary>
        /// <param name="message"></param>
        public GenericEventRunnerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// This allows you to create an exception with the callingEntity and domainEvent type names
        /// </summary>
        /// <param name="message"></param>
        /// <param name="callingEntity"></param>
        /// <param name="domainEvent"></param>
        public GenericEventRunnerException(string message, object callingEntity, IDomainEvent domainEvent)
            : base(message)
        {
            Data.Add("CallingEntityType", callingEntity.GetType().FullName);
            Data.Add("DomainEventType", domainEvent.GetType().FullName);
        }
    }
}