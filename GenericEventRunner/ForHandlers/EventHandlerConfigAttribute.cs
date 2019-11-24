// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace GenericEventRunner.ForHandlers
{
    /// <summary>
    /// TYou can add this attribute to a event handler to override some of the default or configuration settings
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EventHandlerConfigAttribute : Attribute
    {
        /// <summary>
        /// This allows you to alter some of the aspects of a handler
        /// </summary>
        /// <param name="handlerLifetime">This controls the lifetime of a handler when registered in the DI. Default = Transient</param>
        public EventHandlerConfigAttribute(ServiceLifetime handlerLifetime = ServiceLifetime.Transient)
        {
            HandlerLifetime = handlerLifetime;
        }

        /// <summary>
        /// This holds the Lifetime of the class when created by via DI
        /// </summary>
        public ServiceLifetime HandlerLifetime { get; } 

    }
}