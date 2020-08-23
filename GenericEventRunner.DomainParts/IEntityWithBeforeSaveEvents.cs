﻿// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace GenericEventRunner.DomainParts
{
    public interface IEntityWithBeforeSaveEvents
    {
        ICollection<IDomainEvent> GetBeforeSaveEventsThenClear();
    }
}