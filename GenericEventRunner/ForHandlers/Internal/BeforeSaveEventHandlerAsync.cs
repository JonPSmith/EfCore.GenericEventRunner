// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading.Tasks;
using GenericEventRunner.DomainParts;
using StatusGeneric;

namespace GenericEventRunner.ForHandlers.Internal
{
    internal abstract class BeforeSaveEventHandlerAsync
    {
        public abstract Task<IStatusGeneric> HandleAsync(object callingEntity, IEntityEvent entityEvent);
    }

    internal class BeforeSaveHandlerAsync<T> : BeforeSaveEventHandlerAsync
        where T : IEntityEvent
    {
        private readonly IBeforeSaveEventHandlerAsync<T> _handler;

        public BeforeSaveHandlerAsync(IBeforeSaveEventHandlerAsync<T> handler)
        {
            _handler = handler;
        }

        public override Task<IStatusGeneric> HandleAsync(object callingEntity, IEntityEvent entityEvent)
        {
            return _handler.HandleAsync(callingEntity, (T)entityEvent);
        }
    }
}