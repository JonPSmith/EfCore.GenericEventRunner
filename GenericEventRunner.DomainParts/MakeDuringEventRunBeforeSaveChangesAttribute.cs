// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;

namespace GenericEventRunner.DomainParts
{
    /// <summary>
    /// Add this attribute to a During <see cref="IEntityEvent"/> to make the event handler run before SaveChanges
    /// </summary>
    public class MakeDuringEventRunBeforeSaveChangesAttribute : Attribute
    {
    }
}