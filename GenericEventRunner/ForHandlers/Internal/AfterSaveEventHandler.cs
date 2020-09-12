// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Runtime.Serialization;
using GenericEventRunner.DomainParts;

namespace GenericEventRunner.ForHandlers.Internal
{
    internal abstract class AfterSaveEventHandler
    {
        public abstract void Handle(object callingEntity, IEntityEvent entityEvent);
    }

    internal class AfterSaveHandler<T> : AfterSaveEventHandler
        where T : IEntityEvent
    {
        private readonly IAfterSaveEventHandler<T> _handler;

        public AfterSaveHandler(IAfterSaveEventHandler<T> handler)
        {
            _handler = handler;
        }

        public override void Handle(object callingEntity, IEntityEvent entityEvent)
        {
            _handler.Handle(callingEntity, (T)entityEvent);
        }
    }
}