// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;

namespace GenericEventRunner.ForHandlers.Internal
{
    internal static class ValueTaskSyncCheckers
    {
        /// <summary>
        /// This will check the <see cref="ValueTask"/> returned
        /// by a method and ensure it didn't run any async methods.
        /// Also, if the method threw an exception it will throw that exception.
        /// </summary>
        /// <param name="valueTask">The ValueTask from a method that didn't call any async methods</param>
        public static void CheckSyncValueTaskWorked(this ValueTask valueTask)
        {
            if (!valueTask.IsCompleted)
                throw new InvalidOperationException("Expected a sync task, but got an async task");
            if (valueTask.IsFaulted)
                valueTask.GetAwaiter().GetResult();
        }

        /// <summary>
        /// This will check the <see cref="ValueTask{TResult}"/> returned
        /// by a method and ensure it didn't run any async methods.
        /// Also, if the method threw an exception it will throw that exception.
        /// </summary>
        /// <param name="valueTask">The ValueTask from a method that didn't call any async methods</param>
        public static void CheckSyncValueTaskWorked<TResult>(this ValueTask<TResult> valueTask)
        {
            if (!valueTask.IsCompleted)
                throw new InvalidOperationException("Expected a sync task, but got an async task");
            if (valueTask.IsFaulted)
                valueTask.GetAwaiter().GetResult();
        }
    }
}