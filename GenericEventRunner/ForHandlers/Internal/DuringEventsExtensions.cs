// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using GenericEventRunner.DomainParts;
using Microsoft.EntityFrameworkCore;

namespace GenericEventRunner.ForHandlers.Internal
{
    internal static class DuringEventsExtensions
    {

        public static void ClearDuringEvents(this DbContext context)
        {
            context.ChangeTracker.Entries<IEntityWithDuringSaveEvents>().ToList()
                .ForEach(x => x.Entity.ClearDuringSaveEvents());
        }
    }
}