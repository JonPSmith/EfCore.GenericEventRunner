﻿// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using GenericEventRunner.DomainParts;

namespace EntityClasses.DomainEvents
{
    [MakeDuringEventRunBeforeSaveChanges()]
    public class NewBookEventButBeforeSave : IEntityEvent
    {
    }
}