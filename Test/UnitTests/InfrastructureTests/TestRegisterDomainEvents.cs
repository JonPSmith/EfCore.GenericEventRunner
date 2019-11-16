// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DataLayer;
using EntityClasses;
using EntityClasses.DomainEvents;
using EntityClasses.SupportClasses;
using GenericEventRunner.ForDbContext;
using GenericEventRunner.ForHandlers;
using Infrastructure.BeforeEventHandlers;
using Microsoft.Extensions.DependencyInjection;
using Test.EfHelpers;
using Test.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.InfrastructureTests
{
    public class TestRegisterDomainEvents
    {
        [Fact]
        public void TestRegisterEventRunner()
        {
            //SETUP
            var services = new ServiceCollection();

            //ATTEMPT
            services.RegisterEventRunner();

            //VERIFY
            services.Contains(new ServiceDescriptor(typeof(IEventsRunner), typeof(EventsRunner), 
                ServiceLifetime.Scoped), new ServiceDescriptorCompare()).ShouldBeTrue();
        }

        [Fact]
        public void TestRegisterEventHandlers()
        {
            //SETUP
            var services = new ServiceCollection();

            //ATTEMPT
            services.RegisterEventHandlers(Assembly.GetAssembly(typeof(OrderCreatedHandler)));

            //VERIFY
            services.Contains(new ServiceDescriptor(typeof(IBeforeSaveEventHandler<OrderCreatedEvent>), typeof(OrderCreatedHandler),
                ServiceLifetime.Transient), new ServiceDescriptorCompare()).ShouldBeTrue();
        }
    }
}