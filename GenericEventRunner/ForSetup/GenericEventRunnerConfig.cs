// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

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
    }
}