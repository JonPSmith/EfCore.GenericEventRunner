// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License file in the project root for license information.

using System;
using StatusGeneric;

namespace GenericEventRunner.ForDbContext
{
    /// <summary>
    /// This exception is thrown in the overridden SaveChanges/Async method if any of the BeforeSave handlers return errors.
    /// </summary>
    public class GenericEventRunnerStatusException : Exception
    {
        /// <summary>
        /// This 
        /// </summary>
        /// <param name="status">The status returned from the BeforeSave event handlers</param>
        public GenericEventRunnerStatusException(IStatusGeneric status)
            : base($"{status.Message}{Environment.NewLine}{status.GetAllErrors()}")
        {
        }

    }
}