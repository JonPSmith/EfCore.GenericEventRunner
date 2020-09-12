// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using GenericEventRunner.ForSetup;
using Microsoft.Extensions.Logging;
using StatusGeneric;

namespace GenericEventRunner.ForHandlers.Internal
{
    internal class FindRunHandlers
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly IGenericEventRunnerConfig _config;

        private readonly FindHandlers _findHandlers;

        //This is used by During events to handle retry of a transaction
        private readonly Guid _uniqueValue = Guid.NewGuid();



        public FindRunHandlers(IServiceProvider serviceProvider, ILogger logger, IGenericEventRunnerConfig config)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _config = config;

            _findHandlers = new FindHandlers(serviceProvider, logger);
        }

        /// <summary>
        /// This finds either sync or async handlers for the event and runs the handlers with the input event
        /// </summary>
        /// <param name="entityAndEvent"></param>
        /// <param name="loopCount">This gives the loop number for the RunBefore/AfterSaveChangesEvents</param>
        /// <param name="beforeDuringOrAfter">tells you what type of event to find/Run</param>
        /// <param name="allowAsync">true if async is allowed</param>
        /// <returns>Returns a Task containing the combined status from all the event handlers that ran</returns>
        public async ValueTask<IStatusGeneric> RunHandlersForEventAsync(EntityAndEvent entityAndEvent, int loopCount, 
            BeforeDuringOrAfter beforeDuringOrAfter, bool allowAsync)
        {
            var status = new StatusGenericHandler
            {
                Message = "Successfully saved."
            };

            var handlersAndWrappers = _findHandlers.GetHandlers(entityAndEvent, beforeDuringOrAfter, allowAsync);
            foreach (var handlerWrapper in handlersAndWrappers)
            {
                LogEventHandlerRun(loopCount, beforeDuringOrAfter, handlerWrapper);
                if (beforeDuringOrAfter == BeforeDuringOrAfter.BeforeSave)
                {
                    var handlerStatus = handlerWrapper.IsAsync
                        ? await ((BeforeSaveEventHandlerAsync)Activator.CreateInstance(handlerWrapper.WrapperType, handlerWrapper.EventHandler))
                            .HandleAsync(entityAndEvent.CallingEntity, entityAndEvent.EntityEvent).ConfigureAwait(false)
                        : ((BeforeSaveEventHandler)Activator.CreateInstance(handlerWrapper.WrapperType, handlerWrapper.EventHandler))
                            .Handle(entityAndEvent.CallingEntity, entityAndEvent.EntityEvent);
                    if (handlerStatus != null)
                        status.CombineStatuses(handlerStatus);
                }
                else if (beforeDuringOrAfter == BeforeDuringOrAfter.AfterSave)
                {
                    if (handlerWrapper.IsAsync)
                        await ((AfterSaveEventHandlerAsync) Activator.CreateInstance(handlerWrapper.WrapperType,
                                handlerWrapper.EventHandler))
                            .HandleAsync(entityAndEvent.CallingEntity, entityAndEvent.EntityEvent).ConfigureAwait(false);
                    else
                        ((AfterSaveEventHandler) Activator.CreateInstance(handlerWrapper.WrapperType,
                                handlerWrapper.EventHandler))
                            .Handle(entityAndEvent.CallingEntity, entityAndEvent.EntityEvent);
                }
                else
                {
                    //Its either of the during events

                    var handlerStatus = handlerWrapper.IsAsync
                        ? await ((DuringSaveEventHandlerAsync)Activator.CreateInstance(handlerWrapper.WrapperType, handlerWrapper.EventHandler))
                            .HandleAsync(entityAndEvent.CallingEntity, entityAndEvent.EntityEvent, _uniqueValue)
                            .ConfigureAwait(false)
                        : ((DuringSaveEventHandler)Activator.CreateInstance(handlerWrapper.WrapperType, handlerWrapper.EventHandler))
                        .Handle(entityAndEvent.CallingEntity, entityAndEvent.EntityEvent, _uniqueValue);
                    if (handlerStatus != null)
                        status.CombineStatuses(handlerStatus);
                }
            }

            return status;
        }

        private void LogEventHandlerRun(int loopCount, BeforeDuringOrAfter beforeDuringOrAfter, HandlerAndWrapper handlerWrapper)
        {
            _logger.LogInformation(
                $"{beforeDuringOrAfter.ToString()[0]}{loopCount}: About to run a {beforeDuringOrAfter} event handler {handlerWrapper.EventHandler.GetType().FullName}.");
        }
    }
}