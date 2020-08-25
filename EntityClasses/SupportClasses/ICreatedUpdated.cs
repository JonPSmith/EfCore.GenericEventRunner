// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EntityClasses.SupportClasses
{
    public interface ICreatedUpdated      
    {
        DateTime WhenCreatedUtc { get; }
        DateTime LastUpdatedUtc { get; }

        void LogChange(bool added, EntityEntry entry); 
    }

}