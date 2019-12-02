// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;
using StatusGeneric;

namespace GenericEventRunner.ForSetup
{
    /// <summary>
    /// This holds the configuration settings for the GenericEventRunner
    /// NOTE: This is registered as a singleton, i.e. the values cannot be changes dynamically
    /// </summary>
    public class GenericEventRunnerConfig : IGenericEventRunnerConfig
    {
        /// <summary>
        /// This limits the number of times it will look for new events from the BeforeSave events.
        /// This stops circular sets of events
        /// The event runner will throw an exception if the BeforeSave loop goes round move than this number.
        /// </summary>
        public int MaxTimesToLookForBeforeEvents { get; set; } = 6;

        /// <summary>
        /// If this is set to true, then the AfterSave event handlers aren't used
        /// NOTE: This is set to true if the RegisterGenericEventRunner doesn't find any AfterSave event handlers
        /// </summary>
        public bool NotUsingAfterSaveHandlers { get; set; }

        /// <summary>
        /// If true (which is the default value) then the first BeforeSave event handler that returns an error will stop the event runner.
        /// The use cases for each setting is:
        /// true:  Once you have a error, then its not worth going on so stopping quickly is good.
        /// false: If your events have a lot of different checks then this setting gets all the possible errors.
        /// NOTE: Because this is very event-specific you can override this on a per-handler basis via the EventHandlerConfig Attribute
        /// </summary>
        public bool StopOnFirstBeforeHandlerThatHasAnError { get; set; } = true;

        /// <summary>
        /// When SaveChangesWithValidation is called if there is an exception then this method is called (if present)
        /// a) If it returns null then the error is rethrown
        /// b) If it returns a status with errors then those are combined into the GenericEventRunner status
        /// c) If it returns a valid status (i.e. no errors) then it calls SaveChanges again, which might create another exception
        /// Item b) is useful for turning SQL errors into user-friendly error message, and c) is good for handling a DbUpdateConcurrencyException
        /// </summary>
        public Func<Exception, DbContext, IStatusGeneric> SaveChangesExceptionHandler { get; set; }
    }
}