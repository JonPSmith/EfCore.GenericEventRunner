// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GenericEventRunner.ForSetup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StatusGeneric;

namespace GenericEventRunner.ForHandlers.Internal
{
    internal class FindRunHandlers
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly IGenericEventRunnerConfig _config;

        public FindRunHandlers(IServiceProvider serviceProvider, ILogger logger, IGenericEventRunnerConfig config)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _config = config;
        }

        /// <summary>
        /// This finds the handlers for the event and runs the handlers with the input event
        /// </summary>
        /// <param name="entityAndEvent"></param>
        /// <param name="beforeSave">true for BeforeSave, and false for AfterSave</param>
        /// <param name="dontConvertExToStatus">If true then exceptions </param>
        public IStatusGeneric RunHandlersForEvent(EntityAndEvent entityAndEvent, bool beforeSave, bool dontConvertExToStatus)
        {
            var status = new StatusGenericHandler
            {
                Message = "Successfully saved."
            };

            var eventType = entityAndEvent.DomainEvent.GetType();
            var handlerInterface = (beforeSave ? typeof(IBeforeSaveEventHandler<>) : typeof(IAfterSaveEventHandler<>))
                .MakeGenericType(eventType);
            var wrapperType = (beforeSave ? typeof(BeforeSaveHandler<>) : typeof(AfterSaveHandler<>))
                .MakeGenericType(eventType);
            var handlers = _serviceProvider.GetServices(handlerInterface).ToList();

            var beforeAfter = beforeSave ? "BeforeSave" : "AfterSave";
            if (!handlers.Any())
            {
                _logger.LogError($"Missing handler for event of type {eventType.FullName} for {beforeAfter} event handler.");
                throw new GenericEventRunnerException(
                    $"Could not find a {beforeAfter} event handler for the event {eventType.Name}.",
                    entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);
            }

            foreach (var handler in handlers)
            {
                bool HandleException(Exception ex, bool before)
                {
                    if (dontConvertExToStatus)
                        return true;

                    var attr = handler.GetType().GetCustomAttribute<EventHandlerConfigAttribute>();
                    if (attr?.ExceptionErrorString != null || _config.TurnHandlerExceptionsToErrorStatus)
                    {
                        _logger.LogError(
                            $"The {beforeAfter} event handler {eventType.FullName}, but it was turned into a status return.");
                        var errorMessage = attr?.ExceptionErrorString ??
                                           (before
                                               ? _config.DefaultBeforeSaveExceptionErrorString
                                               : _config.DefaultAfterSaveMessageSuffix);
                        if (before)
                        {
                            status.AddError(ex, errorMessage);
                        }
                        else
                        {
                            status.Message = "Successfully saved, but " + errorMessage;
                        }
                    }
                    else
                    {
                        _logger.LogError(ex, $"The {beforeAfter} event handler {eventType.FullName} threw an exception.");
                        return true;
                    }

                    return false;
                }

                _logger.LogInformation($"About to run a {beforeAfter} event handler {handler.GetType().FullName}.");
                if (beforeSave)
                {
                    IStatusGeneric handlerStatus = null;
                    try
                    {
                        var wrappedHandler = (BeforeSaveEventHandler)Activator.CreateInstance(wrapperType, handler);
                        handlerStatus = wrappedHandler.Handle(entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);
                    }
                    catch (Exception ex)
                    {
                        if(HandleException(ex, true))
                            throw;
                    }
                    if (handlerStatus != null)
                        status.CombineStatuses(handlerStatus);
                }
                else
                {
                    try
                    {
                        var wrappedHandler = (AfterSaveEventHandler)Activator.CreateInstance(wrapperType, handler);
                        wrappedHandler.Handle(entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);
                    }
                    catch (Exception ex)
                    {
                        if (HandleException(ex, false))
                            throw;
                    }
                }
            }

            return status;
        }
        
    }
}