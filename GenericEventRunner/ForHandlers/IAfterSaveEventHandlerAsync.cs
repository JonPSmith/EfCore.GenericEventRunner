// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading.Tasks;
using GenericEventRunner.ForEntities;

namespace GenericEventRunner.ForHandlers
{

    public interface IAfterSaveEventHandlerAsync<in T> where T : IDomainEvent
    {
        Task HandleAsync(EntityEvents callingEntity, T domainEvent);
    }
}