// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using GenericEventRunner.DomainParts;

[assembly: InternalsVisibleTo("Test")]

namespace GenericEventRunner.ForHandlers.Internal
{
    internal class EntityAndEvent
    {
        public EntityAndEvent(object callingEntity, IEntityEvent entityEvent)
        {
            CallingEntity = callingEntity ?? throw new ArgumentNullException(nameof(callingEntity));
            EntityEvent = entityEvent ?? throw new ArgumentNullException(nameof(entityEvent));
            HasRemoveDuplicateAttribute = EntityEvent
                .GetType()
                .GetCustomAttribute<RemoveDuplicateEventsAttribute>() != null;
        }

        public object CallingEntity { get; }
        public IEntityEvent EntityEvent { get; }

        public bool HasRemoveDuplicateAttribute { get; }

        private bool Equals(EntityAndEvent other)
        {
            return ReferenceEquals(CallingEntity, other.CallingEntity) && 
                   EntityEvent.GetType() == other.EntityEvent.GetType() &&
                   HasRemoveDuplicateAttribute &&     //Only the same if both has RemoveDuplicateAttribute
                   other.HasRemoveDuplicateAttribute; //Only the same if both has RemoveDuplicateAttribute
        }

        public override bool Equals(object obj)
        {
            return obj is EntityAndEvent other && Equals(other);
        }
        
        //see https://stackoverflow.com/questions/21402465/iequalitycomparer-not-working-as-intended
        public override int GetHashCode()
        {
            return HashCode.Combine(CallingEntity, EntityEvent.GetType(), HasRemoveDuplicateAttribute);
        }
    }
}