// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;

namespace GenericEventRunner.DomainParts
{
    public enum DuringSaveStages {BeforeSaveChanges, AfterSaveChanges}

    /// <summary>
    /// Add this attribute to a <see cref="IDomainEvent"/> to define whether the event is run in the transaction
    /// 1) BeforeSaveChanges: the event handler will be run after the called to ChangeTracker.DetectChanges but before SaveChanges has been called
    /// 2) AfterSaveChanges (default) : the event handler will be run after SaveChanges has been called
    /// </summary>
    public class DuringSaveStageAttribute : Attribute
    {
        public DuringSaveStageAttribute(DuringSaveStages whenToExecute)
        {
            WhenToExecute = whenToExecute;
        }

        public DuringSaveStages WhenToExecute { get; }
    }
}