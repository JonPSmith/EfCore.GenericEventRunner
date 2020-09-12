// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using GenericEventRunner.DomainParts;
using StatusGeneric;

namespace GenericEventRunner.ForHandlers.Internal
{
    internal abstract class BeforeSaveEventHandler
    {
        public abstract IStatusGeneric Handle(object callingEntity, IEntityEvent entityEvent);
    }

    internal class BeforeSaveHandler<T> : BeforeSaveEventHandler
        where T : IEntityEvent
    {
        private readonly IBeforeSaveEventHandler<T> _handler;

        public BeforeSaveHandler(IBeforeSaveEventHandler<T> handler)
        {
            _handler = handler;
        }

        public override IStatusGeneric Handle(object callingEntity, IEntityEvent entityEvent)
        {
            return _handler.Handle(callingEntity, (T)entityEvent);
        }
    }
}