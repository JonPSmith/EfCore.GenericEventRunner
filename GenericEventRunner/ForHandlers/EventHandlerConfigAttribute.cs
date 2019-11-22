// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace GenericEventRunner.ForHandlers
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EventHandlerConfigAttribute : Attribute
    {
        /// <summary>
        /// This allows you to alter some of the aspects of a handler
        /// </summary>
        /// <param name="exceptionErrorString">If set then a) an exception will be turned into a status, and b) the error message will use this string
        /// a) BeforeSave handlers: this string will be used in an error message.
        /// b) AfterSave handlers: this string is added to the end of the message, which starts with "Successfully saved, but "</param>
        /// <param name="handlerLifetime"></param>
        public EventHandlerConfigAttribute(string exceptionErrorString = null, ServiceLifetime handlerLifetime = ServiceLifetime.Transient)
        {
            ExceptionErrorString = exceptionErrorString;
            HandlerLifetime = handlerLifetime;
        }

        public string ExceptionErrorString { get; }

        public ServiceLifetime HandlerLifetime { get; } 

    }
}