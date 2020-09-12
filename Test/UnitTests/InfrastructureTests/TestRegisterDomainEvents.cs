// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using GenericEventRunner.ForHandlers;
using GenericEventRunner.ForSetup;
using Microsoft.Extensions.DependencyInjection;
using Test.EventsAndHandlers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.InfrastructureTests
{
    public class TestRegisterDomainEvents
    {

        [Fact]
        public void TestRegisterTwiceBad()
        {
            //SETUP
            var services = new ServiceCollection();

            //ATTEMPT
            services.AddTransient<TestRegisterDomainEvents>();
            services.AddTransient<TestRegisterDomainEvents>();

            //VERIFY
            services.Count.ShouldEqual(2);
        }

        [Fact]
        public void TestRegisterEventHandlers()
        {
            //SETUP
            var services = new ServiceCollection();

            //ATTEMPT
            services.RegisterGenericEventRunner(Assembly.GetAssembly(typeof(BeforeHandlerThrowsExceptionWithAttribute)));

            //VERIFY
            //Before event handlers
            services.Contains(new ServiceDescriptor(typeof(IBeforeSaveEventHandler<EventCircularEvent>),
                typeof(BeforeHandlerCircularEvent),
                ServiceLifetime.Transient), new ServiceDescriptorCompare()).ShouldBeTrue();
            services.Contains(new ServiceDescriptor(typeof(IBeforeSaveEventHandler<EventTestExceptionHandlerWithAttribute>), 
                typeof(BeforeHandlerThrowsExceptionWithAttribute),
                ServiceLifetime.Scoped), new ServiceDescriptorCompare()).ShouldBeTrue();

            //During event handlers
            services.Contains(new ServiceDescriptor(typeof(IDuringSaveEventHandler<EventDoNothing>),
                typeof(DuringHandlerDoNothing),
                ServiceLifetime.Transient), new ServiceDescriptorCompare()).ShouldBeTrue();

            //After event handlers
            services.Contains(new ServiceDescriptor(typeof(IAfterSaveEventHandler<EventTestAfterExceptionHandler>),
                typeof(AfterHandlerThrowsException),
                ServiceLifetime.Transient), new ServiceDescriptorCompare()).ShouldBeTrue();
            //Async event handlers
            services.Contains(new ServiceDescriptor(typeof(IBeforeSaveEventHandlerAsync<EventDoNothing>),
                typeof(BeforeHandlerDoNothingAsync),
                ServiceLifetime.Transient), new ServiceDescriptorCompare()).ShouldBeTrue();
            services.Contains(new ServiceDescriptor(typeof(IAfterSaveEventHandlerAsync<EventDoNothing>),
                typeof(AfterHandlerDoNothingAsync),
                ServiceLifetime.Transient), new ServiceDescriptorCompare()).ShouldBeTrue();

        }

        [Fact]
        public void TestRegisterEventHandlersWithConfigTuningOffDuringHandlers()
        {
            //SETUP
            var services = new ServiceCollection();
            var config = new GenericEventRunnerConfig {NotUsingDuringSaveHandlers = true};

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
                ServiceLifetime.Transient), new ServiceDescriptorCompare()).ShouldBeTrue();
            services.Contains(new ServiceDescriptor(typeof(IAfterSaveEventHandlerAsync<EventDoNothing>),
                typeof(AfterHandlerDoNothingAsync),
                ServiceLifetime.Transient), new ServiceDescriptorCompare()).ShouldBeTrue();

            //During event handlers
            services.Contains(new ServiceDescriptor(typeof(IDuringSaveEventHandler<EventDoNothing>),
                typeof(DuringHandlerDoNothing),
                ServiceLifetime.Transient), new ServiceDescriptorCompare()).ShouldBeFalse();
        }

        [Fact]
        public void TestRegisterEventHandlersWithConfigTuningOffAfterHandlers()
        {
            //SETUP
            var services = new ServiceCollection();
            var config = new GenericEventRunnerConfig { NotUsingAfterSaveHandlers = true };

            //ATTEMPT
            services.RegisterGenericEventRunner(config, Assembly.GetAssembly(typeof(BeforeHandlerThrowsExceptionWithAttribute)));

            //VERIFY
            services.Contains(new ServiceDescriptor(typeof(IBeforeSaveEventHandler<EventCircularEvent>),
                typeof(BeforeHandlerCircularEvent),
                ServiceLifetime.Transient), new ServiceDescriptorCompare()).ShouldBeTrue();
            services.Contains(new ServiceDescriptor(typeof(IBeforeSaveEventHandler<EventTestExceptionHandlerWithAttribute>),
                typeof(BeforeHandlerThrowsExceptionWithAttribute),
                ServiceLifetime.Scoped), new ServiceDescriptorCompare()).ShouldBeTrue();

            //During event handlers
            services.Contains(new ServiceDescriptor(typeof(IDuringSaveEventHandler<EventDoNothing>),
                typeof(DuringHandlerDoNothing),
                ServiceLifetime.Transient), new ServiceDescriptorCompare()).ShouldBeTrue();

            //after event handlers
            services.Contains(new ServiceDescriptor(typeof(IAfterSaveEventHandler<EventTestAfterExceptionHandler>),
                typeof(AfterHandlerThrowsException),
                ServiceLifetime.Transient), new ServiceDescriptorCompare()).ShouldBeFalse();
            services.Contains(new ServiceDescriptor(typeof(IAfterSaveEventHandlerAsync<EventDoNothing>),
                typeof(AfterHandlerDoNothingAsync),
                ServiceLifetime.Transient), new ServiceDescriptorCompare()).ShouldBeFalse();
        }

        [Fact]
        public void TestRegisterEventHandlersTwiceBad()
        {
            //SETUP
            var services = new ServiceCollection();

            //ATTEMPT
            services.RegisterGenericEventRunner(Assembly.GetAssembly(typeof(BeforeHandlerThrowsExceptionWithAttribute)));
            var ex = Assert.Throws<InvalidOperationException>(() =>
                services.RegisterGenericEventRunner(
                    Assembly.GetAssembly(typeof(BeforeHandlerThrowsExceptionWithAttribute))));

            //VERIFY
            ex.Message.ShouldEqual("You can only call this method once to register the GenericEventRunner and event handlers.");
        }
    }
}