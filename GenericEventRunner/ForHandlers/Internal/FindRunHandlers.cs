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

        private readonly FindHandlers _findHandlers;

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
        /// <param name="beforeSave">true for BeforeSave, and false for AfterSave</param>
        /// <param name="allowAsync">true if async is allowed</param>
        /// <returns>Returns a Task containing the combined status from all the event handlers that ran</returns>
        public async ValueTask<IStatusGeneric> RunHandlersForEventAsync(EntityAndEvent entityAndEvent, int loopCount, bool beforeSave, bool allowAsync)
        {
            var status = new StatusGenericHandler
            {
                Message = "Successfully saved."
            };

            var handlersAndWrappers = _findHandlers.GetHandlers(entityAndEvent, beforeSave, allowAsync);
            foreach (var handlerWrapper in handlersAndWrappers)
            {
                LogEventHandlerRun(loopCount, beforeSave, handlerWrapper);
                if (beforeSave)
                {
                    var handlerStatus = handlerWrapper.IsAsync
                        ? await ((BeforeSaveEventHandlerAsync)Activator.CreateInstance(handlerWrapper.WrapperType, handlerWrapper.EventHandler))
                            .HandleAsync(entityAndEvent.CallingEntity, entityAndEvent.DomainEvent).ConfigureAwait(false)
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
                            .HandleAsync(entityAndEvent.CallingEntity, entityAndEvent.DomainEvent).ConfigureAwait(false);
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