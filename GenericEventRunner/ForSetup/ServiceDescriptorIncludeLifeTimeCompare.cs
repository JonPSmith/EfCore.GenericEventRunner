// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace GenericEventRunner.ForSetup
{
    public class ServiceDescriptorIncludeLifeTimeCompare : IEqualityComparer<ServiceDescriptor>
    {
        public bool Equals(ServiceDescriptor x, ServiceDescriptor y)
        {
            return x.ServiceType == y.ServiceType
                   && x.ImplementationType == y.ImplementationType
                   && x.Lifetime == y.Lifetime;
        }

        public int GetHashCode(ServiceDescriptor obj)
        {
            throw new NotImplementedException();
        }
    }
}
