// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using GenericEventRunner.ForHandlers;
using GenericEventRunner.ForSetup;
using Microsoft.Extensions.DependencyInjection;
using Test.EventsAndHandlers;
using Test.Helpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.InfrastructureTests
{
    public class TestRegisterDomainEvents
    {

        [Fact]
        public void TestRegisterEventHandlers()
        {
            //SETUP
            var services = new ServiceCollection();

            //ATTEMPT
            services.RegisterGenericEventRunner(Assembly.GetAssembly(typeof(BeforeHandlerThrowsExceptionWithAttribute)));

            //VERIFY
            services.Contains(new ServiceDescriptor(typeof(IBeforeSaveEventHandler<EventCircularEvent>),
                typeof(BeforeHandlerCircularEvent),
                ServiceLifetime.Transient), new ServiceDescriptorCompare()).ShouldBeTrue();
            services.Contains(new ServiceDescriptor(typeof(IBeforeSaveEventHandler<EventTestExceptionHandlerWithAttribute>), 
                typeof(BeforeHandlerThrowsExceptionWithAttribute),
                ServiceLifetime.Scoped), new ServiceDescriptorCompare()).ShouldBeTrue();
            services.Contains(new ServiceDescriptor(typeof(IAfterSaveEventHandler<EventTestAfterExceptionHandler>),
                typeof(AfterHandlerThrowsException),
                ServiceLifetime.Transient), new ServiceDescriptorCompare()).ShouldBeTrue();
        }

        [Fact]
        public void TestRegisterEventHandlersWithConfigTuningOffAfterHandlers()
        {
            //SETUP
            var services = new ServiceCollection();
            var config = new GenericEventRunnerConfig {NotUsingAfterSaveHandlers = true};

            //ATTEMPT
            services.RegisterGenericEventRunner(config, Assembly.GetAssembly(typeof(BeforeHandlerThrowsExceptionWithAttribute)));

            //VERIFY
            services.Contains(new ServiceDescriptor(typeof(IBeforeSaveEventHandler<EventCircularEvent>),
                typeof(BeforeHandlerCircularEvent),
                ServiceLifetime.Transient), new ServiceDescriptorCompare()).ShouldBeTrue();
            services.Contains(new ServiceDescriptor(typeof(IBeforeSaveEventHandler<EventTestExceptionHandlerWithAttribute>),
                typeof(BeforeHandlerThrowsExceptionWithAttribute),
                ServiceLifetime.Scoped), new ServiceDescriptorCompare()).ShouldBeTrue();
            services.Contains(new ServiceDescriptor(typeof(IAfterSaveEventHandler<EventTestAfterExceptionHandler>),
                typeof(AfterHandlerThrowsException),
                ServiceLifetime.Transient), new ServiceDescriptorCompare()).ShouldBeFalse();
        }
    }
}