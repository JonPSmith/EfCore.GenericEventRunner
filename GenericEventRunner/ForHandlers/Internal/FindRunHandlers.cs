// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
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

        private FindHandlers _findHandlers;

        public FindRunHandlers(IServiceProvider serviceProvider, ILogger logger, IGenericEventRunnerConfig config)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _config = config;

            _findHandlers = new FindHandlers(serviceProvider, logger);
        }

        /// <summary>
        /// This finds the handlers for the event and runs the handlers with the input event
        /// </summary>
        /// <param name="entityAndEvent"></param>
        /// <param name="loopCount">This gives the loop number for the RunBefore/AfterSaveChangesEvents</param>
        /// <param name="beforeSave">true for BeforeSave, and false for AfterSave</param>
        /// <returns>Returns a combined status from all the event handlers that ran</returns>
        public IStatusGeneric RunHandlersForEvent(EntityAndEvent entityAndEvent, int loopCount, bool beforeSave)
        {
            var status = new StatusGenericHandler
            {
                Message = "Successfully saved."
            };

            var handlersAndWrappers = _findHandlers.GetHandlers(entityAndEvent, beforeSave, false);
            foreach (var handlerWrapper in handlersAndWrappers)
            {
                LogEventHandlerRun(loopCount, beforeSave, handlerWrapper);
                if (beforeSave)
                {
                    var handlerStatus = ((BeforeSaveEventHandler)Activator.CreateInstance(handlerWrapper.WrapperType, handlerWrapper.EventHandler))
                        .Handle(entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);
                    if (handlerStatus != null)
                        status.CombineStatuses(handlerStatus);
                }
                else
                {
                    ((AfterSaveEventHandler)Activator.CreateInstance(handlerWrapper.WrapperType, handlerWrapper.EventHandler))
                        .Handle(entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);
                }
            }

            return status;
        }

        /// <summary>
        /// This finds either sync or async handlers for the event and runs the handlers with the input event
        /// </summary>
        /// <param name="entityAndEvent"></param>
        /// <param name="loopCount">This gives the loop number for the RunBefore/AfterSaveChangesEvents</param>
        /// <param name="beforeSave">true for BeforeSave, and false for AfterSave</param>
        /// <returns>Returns a Task containing the combined status from all the event handlers that ran</returns>
        public async Task<IStatusGeneric> RunHandlersForEventAsync(EntityAndEvent entityAndEvent, int loopCount, bool beforeSave)
        {
            var status = new StatusGenericHandler
            {
                Message = "Successfully saved."
            };

            var handlersAndWrappers = _findHandlers.GetHandlers(entityAndEvent, beforeSave, true);
            foreach (var handlerWrapper in handlersAndWrappers)
            {
                LogEventHandlerRun(loopCount, beforeSave, handlerWrapper);
                if (beforeSave)
                {
                    var handlerStatus = handlerWrapper.IsAsync
                        ? await((BeforeSaveEventHandlerAsync)Activator.CreateInstance(handlerWrapper.WrapperType, handlerWrapper.EventHandler))
                            .HandleAsync(entityAndEvent.CallingEntity, entityAndEvent.DomainEvent)
                        : ((BeforeSaveEventHandler)Activator.CreateInstance(handlerWrapper.WrapperType, handlerWrapper.EventHandler))
                        .Handle(entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);
                    if (handlerStatus != null)
                        status.CombineStatuses(handlerStatus);
                }
                else
                {
                    if (handlerWrapper.IsAsync)
                        await ((AfterSaveEventHandlerAsync) Activator.CreateInstance(handlerWrapper.WrapperType,
                                handlerWrapper.EventHandler))
                            .HandleAsync(entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);
                    else
                        ((AfterSaveEventHandler) Activator.CreateInstance(handlerWrapper.WrapperType,
                                handlerWrapper.EventHandler))
                            .Handle(entityAndEvent.CallingEntity, entityAndEvent.DomainEvent);
                }
            }

            return status;
        }

        private void LogEventHandlerRun(int loopCount, bool beforeSave, HandlerAndWrapper handlerWrapper)
        {
            var beforeAfter = beforeSave ? "BeforeSave" : "AfterSave";
            _logger.LogInformation(
                $"{beforeAfter[0]}{loopCount}: About to run a {beforeAfter} event handler {handlerWrapper.EventHandler.GetType().FullName}.");
        }
    }
}