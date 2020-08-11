// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;

namespace GenericEventRunner.ForHandlers.Internal
{
    internal class HandlerAndWrapper
    {
        public HandlerAndWrapper(object eventHandler, Type eventType, bool beforeSave, bool isAsync)
        {
            EventHandler = eventHandler;
            IsAsync = isAsync;

            switch (beforeSave, isAsync)
            {
                case (false, false):
                    WrapperType = typeof(AfterSaveHandler<>).MakeGenericType(eventType);
                    break;
                case (true, false):
                    WrapperType = typeof(BeforeSaveHandler<>).MakeGenericType(eventType);
                    break;
                case (false, true):
                    WrapperType = typeof(AfterSaveHandlerAsync<>).MakeGenericType(eventType);
                    break;
                case (true, true):
                    WrapperType = typeof(BeforeSaveHandlerAsync<>).MakeGenericType(eventType);
                    break;
            }
        }

        public object EventHandler { get; }

        public Type WrapperType { get; }

        public bool IsAsync { get; }
    }
}