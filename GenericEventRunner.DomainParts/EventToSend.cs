// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace GenericEventRunner.DomainParts
{
    /// <summary>
    /// This allows you to control which list, the BeforeSave and AfterSave, to add the event to.
    /// </summary>
    public enum EventToSend
    {
        /// <summary>
        /// This puts an event into BeforeSaveEvents list
        /// </summary>
        BeforeSave,
        /// <summary>
        /// This puts an event into the DuringSaveEvents list
        /// </summary>
        DuringSave,
        /// <summary>
        /// This puts an event into AfterSaveEvents list
        /// </summary>
        AfterSave,
        /// <summary>
        /// This puts an event into both the BeforeSaveEvents list and the AfterSaveEvents list
        /// </summary>
        BeforeAndAfterSave

    }
}