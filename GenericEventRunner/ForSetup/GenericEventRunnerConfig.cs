// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace GenericEventRunner.ForSetup
{
    public class GenericEventRunnerConfig
    {
        /// <summary>
        /// This limits the number of times it will look for new events from the BeforeSave events.
        /// This stops circular sets of events
        /// The event runner will throw an exception if the BeforeSave loop goes round move than this number.
        /// </summary>
        public int MaxTimesToLookForBeforeEvents { get; set; } = 6;


        /// <summary>
        /// If a handler isn't found for an event, then the event runner will throw an event
        /// Setting this property to true will suppress that exception.
        /// </summary>
        public bool DoNotThrowExceptionIfNoHandlerForAnEvent { get; set; }
    }
}