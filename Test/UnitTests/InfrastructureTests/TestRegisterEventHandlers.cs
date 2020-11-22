// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using EntityClasses.DomainEvents;
using GenericEventRunner.ForHandlers;
using GenericEventRunner.ForSetup;
using Microsoft.Extensions.DependencyInjection;
using Test.EventsAndHandlers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.InfrastructureTests
{
    public class TestRegisterEventHandlers
    {
        private readonly ITestOutputHelper _output;

        public TestRegisterEventHandlers(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestRegisterTwiceBad()
        {
            //SETUP
            var services = new ServiceCollection();

            //ATTEMPT
            services.AddTransient<TestRegisterEventHandlers>();
            services.AddTransient<TestRegisterEventHandlers>();

            //VERIFY
            services.Count.ShouldEqual(2);
        }

        [Fact]
        public void TestRegisterEventHandlersNormalOk()
        {
            //SETUP
            var services = new ServiceCollection();

            //ATTEMPT
            var logs = services.RegisterGenericEventRunner(Assembly
                .GetAssembly(typeof(BeforeHandlerThrowsExceptionWithAttribute)));

            //VERIFY
            //Before event handlers
            services.Contains(new ServiceDescriptor(typeof(IBeforeSaveEventHandler<EventCircularEvent>),
                typeof(BeforeHandlerCircularEvent),
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();
            services.Contains(new ServiceDescriptor(typeof(IBeforeSaveEventHandler<EventTestExceptionHandlerWithAttribute>), 
                typeof(BeforeHandlerThrowsExceptionWithAttribute),
                ServiceLifetime.Scoped), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();

            //During event handlers
            services.Contains(new ServiceDescriptor(typeof(IDuringSaveEventHandler<EventDoNothing>),
                typeof(DuringHandlerDoNothing),
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();

            //After event handlers
            services.Contains(new ServiceDescriptor(typeof(IAfterSaveEventHandler<EventTestAfterExceptionHandler>),
                typeof(AfterHandlerThrowsException),
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();
            //Async event handlers
            services.Contains(new ServiceDescriptor(typeof(IBeforeSaveEventHandlerAsync<EventDoNothing>),
                typeof(BeforeHandlerDoNothingAsync),
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();
            services.Contains(new ServiceDescriptor(typeof(IAfterSaveEventHandlerAsync<EventDoNothing>),
                typeof(AfterHandlerDoNothingAsync),
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();

            foreach (var log in logs)
            {
                _output.WriteLine(log);
            }
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
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();
            services.Contains(new ServiceDescriptor(typeof(IBeforeSaveEventHandler<EventTestExceptionHandlerWithAttribute>),
                typeof(BeforeHandlerThrowsExceptionWithAttribute),
                ServiceLifetime.Scoped), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();
            services.Contains(new ServiceDescriptor(typeof(IAfterSaveEventHandler<EventTestAfterExceptionHandler>),
                typeof(AfterHandlerThrowsException),
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();
            services.Contains(new ServiceDescriptor(typeof(IAfterSaveEventHandlerAsync<EventDoNothing>),
                typeof(AfterHandlerDoNothingAsync),
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();

            //During event handlers
            services.Contains(new ServiceDescriptor(typeof(IDuringSaveEventHandler<EventDoNothing>),
                typeof(DuringHandlerDoNothing),
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeFalse();
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
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();
            services.Contains(new ServiceDescriptor(typeof(IBeforeSaveEventHandler<EventTestExceptionHandlerWithAttribute>),
                typeof(BeforeHandlerThrowsExceptionWithAttribute),
                ServiceLifetime.Scoped), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();

            //During event handlers
            services.Contains(new ServiceDescriptor(typeof(IDuringSaveEventHandler<EventDoNothing>),
                typeof(DuringHandlerDoNothing),
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();

            //after event handlers
            services.Contains(new ServiceDescriptor(typeof(IAfterSaveEventHandler<EventTestAfterExceptionHandler>),
                typeof(AfterHandlerThrowsException),
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeFalse();
            services.Contains(new ServiceDescriptor(typeof(IAfterSaveEventHandlerAsync<EventDoNothing>),
                typeof(AfterHandlerDoNothingAsync),
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeFalse();
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