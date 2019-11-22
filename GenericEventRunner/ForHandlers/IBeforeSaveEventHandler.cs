// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using GenericEventRunner.ForEntities;
using StatusGeneric;

namespace GenericEventRunner.ForHandlers
{
    public interface IBeforeSaveEventHandler<in T> where T : IDomainEvent
    {
        IStatusGeneric Handle(EntityEvents callingEntity, T domainEvent);
    }
}