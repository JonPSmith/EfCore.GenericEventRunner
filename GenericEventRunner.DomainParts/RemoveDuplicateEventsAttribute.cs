// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;

namespace GenericEventRunner.DomainParts
{
    /// <summary>
    /// Add this attribute to a <see cref="IEntityEvent"/> and the EventRunner will remove events that a) have the same type, and b) come from the same entity
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RemoveDuplicateEventsAttribute : Attribute
    {
        
    }
}