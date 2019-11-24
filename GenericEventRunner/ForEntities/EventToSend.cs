// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace GenericEventRunner.ForEntities
{
    /// <summary>
    /// This allows you to control which list, the BeforeSave and AfterSave, to add the event to.
    /// </summary>
    public enum EventToSend
    {
        Before, After, Both
    }
}