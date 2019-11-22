// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.
namespace GenericEventRunner.ForSetup
{
    public interface IGenericEventRunnerConfig
    {
        /// <summary>
        /// This limits the number of times it will look for new events from the BeforeSave events.
        /// This stops circular sets of events
        /// The event runner will throw an exception if the BeforeSave loop goes round move than this number.
        /// </summary>
        int MaxTimesToLookForBeforeEvents { get; }

        /// <summary>
        /// If a handler throws an exception when SaveChangesWithStatus/Async is called and this property is true,
        ///    then the Exception will be turned into a IStatusGeneric status
        /// a) For BeforeSave event handlers an error is added to the status
        /// b) For AfterSave event handlers the IStatusGeneric Message is changed to say that the database was updated, but an AfterSave handler failed
        /// </summary>
        bool TurnHandlerExceptionsToErrorStatus { get; }

        /// <summary>
        /// If TurnHandlerExceptionsToErrorStatus is true and a BeforeSave event handlers has an exception, then this message will be added as an error in the status.
        /// NOTE: The EventHandlerConfigAttribute.ExceptionErrorString overrides this
        /// </summary>
        string DefaultBeforeSaveExceptionErrorString { get; }

        /// <summary>
        /// If TurnHandlerExceptionsToErrorStatus is true and an AfterSave event handlers has an exception, then this message will added to the end of the status message.
        /// The prefix of the message is "Successfully saved, but ". 
        /// NOTE: The EventHandlerConfigAttribute.ExceptionErrorString overrides this
        /// </summary>
        string DefaultAfterSaveMessageSuffix { get; }
    }
}