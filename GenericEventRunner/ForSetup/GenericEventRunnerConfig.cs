// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace GenericEventRunner.ForSetup
{
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
        /// If a handler throws an exception when SaveChangesWithStatus/Async is called and this property is true,
        ///    then the Exception will be turned into a IStatusGeneric status
        /// a) For BeforeSave event handlers an error is added to the status
        /// b) For AfterSave event handlers the IStatusGeneric Message is changed to say that the database was updated, but an AfterSave handler failed
        /// </summary>
        public bool TurnHandlerExceptionsToErrorStatus { get; set; } = true;

        /// <summary>
        /// If TurnHandlerExceptionsToErrorStatus is true and a BeforeSave event handlers has an exception, then this message will be added as an error in the status.
        /// NOTE: The EventHandlerConfigAttribute.ExceptionErrorString overrides this
        /// </summary>
        public string DefaultBeforeSaveExceptionErrorString { get; set; } =
            "There was a system error. If this persists then please contact us.";

        /// <summary>
        /// If TurnHandlerExceptionsToErrorStatus is true and an AfterSave event handlers has an exception, then this message will added to the end of the status message.
        /// The prefix of the message is "Successfully saved, but ". 
        /// NOTE: The EventHandlerConfigAttribute.ExceptionErrorString overrides this
        /// </summary>
        public string DefaultAfterSaveMessageSuffix { get; set; } = "it failed to sent a update report.";
    }
}