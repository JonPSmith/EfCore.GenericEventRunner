// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License file in the project root for license information.

using StatusGeneric;

namespace GenericEventRunner.ForDbContext
{
    /// <summary>
    /// Interface to access Status filled in by EventsRunner
    /// </summary>
    public interface IStatusFromLastSaveChanges
    {
        /// <summary>
        /// This returns the Status of last SaveChanges/Async and SaveChangesWithStatus/Async done by the GenericEventRunner
        /// Useful if you are capturing the GenericEventRunnerStatusException and want to get the Status that goes with it. 
        /// NOTE: This is null if no event handler is provided, or SaveChanges/Async etc. have not been called yet.
        /// </summary>
        IStatusGeneric<int> StatusFromLastSaveChanges { get;  }
    }
}