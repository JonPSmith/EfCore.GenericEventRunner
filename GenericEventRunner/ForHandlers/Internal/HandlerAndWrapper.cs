// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;

namespace GenericEventRunner.ForHandlers.Internal
{
    internal class HandlerAndWrapper
    {
        public HandlerAndWrapper(object eventHandler, Type eventType, BeforeDuringOrAfter beforeDuringOrAfter, bool isAsync)
        {
            EventHandler = eventHandler;
            IsAsync = isAsync;

            switch (beforeDuringOrAfter, isAsync)
            {
                case (BeforeDuringOrAfter.BeforeSave, false):
                    WrapperType = typeof(BeforeSaveHandler<>).MakeGenericType(eventType);
                    break;
                case (BeforeDuringOrAfter.BeforeSave, true):
                    WrapperType = typeof(BeforeSaveHandlerAsync<>).MakeGenericType(eventType);
                    break;
                case (BeforeDuringOrAfter.DuringButBeforeSaveChanges, false):
                case (BeforeDuringOrAfter.DuringSave, false):
                    WrapperType = typeof(DuringSaveHandler<>).MakeGenericType(eventType);
                    break;
                case (BeforeDuringOrAfter.DuringButBeforeSaveChanges, true):
                case (BeforeDuringOrAfter.DuringSave, true):
                    WrapperType = typeof(DuringSaveHandlerAsync<>).MakeGenericType(eventType);
                    break;
                case (BeforeDuringOrAfter.AfterSave, false):
                    WrapperType = typeof(AfterSaveHandler<>).MakeGenericType(eventType);
                    break;
                case (BeforeDuringOrAfter.AfterSave, true):
                    WrapperType = typeof(AfterSaveHandlerAsync<>).MakeGenericType(eventType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object EventHandler { get; }

        public Type WrapperType { get; }
        public bool IsAsync { get; }
    }
}