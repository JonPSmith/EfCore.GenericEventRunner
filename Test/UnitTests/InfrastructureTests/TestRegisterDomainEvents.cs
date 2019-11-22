// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using EntityClasses.DomainEvents;
using GenericEventRunner.ForDbContext;
using GenericEventRunner.ForHandlers;
using GenericEventRunner.ForSetup;
using Infrastructure.BeforeEventHandlers;
using Microsoft.Extensions.DependencyInjection;
using Test.Helpers;
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
            var config = new GenericEventRunnerConfig();

            //ATTEMPT
            services.RegisterEventRunner(config);

            //VERIFY
            services.Contains(new ServiceDescriptor(typeof(IEventsRunner), typeof(EventsRunner), 
                ServiceLifetime.Scoped), new ServiceDescriptorCompare()).ShouldBeTrue();
            services.Contains(new ServiceDescriptor(typeof(IGenericEventRunnerConfig), config), 
                new ServiceDescriptorCompare()).ShouldBeTrue();
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