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
    }
}