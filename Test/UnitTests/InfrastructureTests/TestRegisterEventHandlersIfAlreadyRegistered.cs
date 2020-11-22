// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using EntityClasses.DomainEvents;
using GenericEventRunner.ForHandlers;
using GenericEventRunner.ForSetup;
using Microsoft.Extensions.DependencyInjection;
using OnlyAfterHandlers;
using OnlyBeforeHandlers;
using OnlyDuringHandlers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.InfrastructureTests
{
    public class TestRegisterEventHandlersIfAlreadyRegistered
    {
        private readonly ITestOutputHelper _output;

        public TestRegisterEventHandlersIfAlreadyRegistered(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestRegisterEventHandlersThreeProjects()
        {
            //SETUP
            var services = new ServiceCollection();
            var config = new GenericEventRunnerConfig();

            //ATTEMPT
            var logs = services.RegisterGenericEventRunner(config,
                Assembly.GetAssembly(typeof(BeforeHandler)),
                Assembly.GetAssembly(typeof(DuringHandler)),
                Assembly.GetAssembly(typeof(AfterHandler))
                );

            //VERIFY
            services.Contains(new ServiceDescriptor(typeof(IBeforeSaveEventHandler<OrderCreatedEvent>),
                typeof(BeforeHandler),
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();
            services.Contains(new ServiceDescriptor(typeof(IDuringSaveEventHandler<NewBookEvent>),
                typeof(DuringHandler),
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();
            services.Contains(new ServiceDescriptor(typeof(IAfterSaveEventHandler<OrderReadyToDispatchEvent>),
                typeof(AfterHandler),
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();

            foreach (var log in logs)
            {
                _output.WriteLine(log);
            }

            config.NotUsingDuringSaveHandlers.ShouldBeFalse();
            config.NotUsingAfterSaveHandlers.ShouldBeFalse();
            services.Count.ShouldEqual(5);
        }


        [Fact]
        public void TestRegisterEventHandlersOnlyBefore()
        {
            //SETUP
            var services = new ServiceCollection();
            var config = new GenericEventRunnerConfig();

            //ATTEMPT
            var logs = services.RegisterGenericEventRunner(config,
                Assembly.GetAssembly(typeof(BeforeHandler))
            );

            //VERIFY
            services.Contains(new ServiceDescriptor(typeof(IBeforeSaveEventHandler<OrderCreatedEvent>),
                typeof(BeforeHandler),
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();

            foreach (var log in logs)
            {
                _output.WriteLine(log);
            }

            config.NotUsingDuringSaveHandlers.ShouldBeTrue();
            config.NotUsingAfterSaveHandlers.ShouldBeTrue();
            services.Count.ShouldEqual(3);
        }

        [Fact]
        public void TestRegisterEventHandlersBeforeAlreadyRegisteredThreeProjects()
        {
            //SETUP
            var services = new ServiceCollection();
            var config = new GenericEventRunnerConfig();
            services.AddTransient<IBeforeSaveEventHandler<OrderCreatedEvent>, BeforeHandler>();

            //ATTEMPT
            var logs = services.RegisterGenericEventRunner(config,
                Assembly.GetAssembly(typeof(BeforeHandler)),
                Assembly.GetAssembly(typeof(DuringHandler)),
                Assembly.GetAssembly(typeof(AfterHandler))
            );

            //VERIFY
            services.Contains(new ServiceDescriptor(typeof(IBeforeSaveEventHandler<OrderCreatedEvent>),
                typeof(BeforeHandler),
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();
            services.Contains(new ServiceDescriptor(typeof(IDuringSaveEventHandler<NewBookEvent>),
                typeof(DuringHandler),
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();
            services.Contains(new ServiceDescriptor(typeof(IAfterSaveEventHandler<OrderReadyToDispatchEvent>),
                typeof(AfterHandler),
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();

            foreach (var log in logs)
            {
                _output.WriteLine(log);
            }

            config.NotUsingDuringSaveHandlers.ShouldBeFalse();
            config.NotUsingAfterSaveHandlers.ShouldBeFalse();
            services.Count.ShouldEqual(5);
        }

        [Fact]
        public void TestRegisterEventHandlersBeforeAlreadyRegisteredJustBefore()
        {
            //SETUP
            var services = new ServiceCollection();
            var config = new GenericEventRunnerConfig();
            services.AddTransient<IBeforeSaveEventHandler<OrderCreatedEvent>, BeforeHandler>();

            //ATTEMPT
            var logs = services.RegisterGenericEventRunner(config,
                Assembly.GetAssembly(typeof(BeforeHandler))
            );

            //VERIFY
            services.Contains(new ServiceDescriptor(typeof(IBeforeSaveEventHandler<OrderCreatedEvent>),
                typeof(BeforeHandler),
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue(); ;

            foreach (var log in logs)
            {
                _output.WriteLine(log);
            }

            config.NotUsingDuringSaveHandlers.ShouldBeTrue();
            config.NotUsingAfterSaveHandlers.ShouldBeTrue();
            services.Count.ShouldEqual(3);
        }

        [Fact]
        public void TestRegisterEventHandlersAllAlreadyRegisteredThreeProjects()
        {
            //SETUP
            var services = new ServiceCollection();
            var config = new GenericEventRunnerConfig();
            services.AddTransient<IBeforeSaveEventHandler<OrderCreatedEvent>, BeforeHandler>();
            services.AddTransient<IDuringSaveEventHandler<NewBookEvent>, DuringHandler>();
            services.AddTransient<IAfterSaveEventHandler<OrderReadyToDispatchEvent>, AfterHandler>();

            //ATTEMPT
            var logs = services.RegisterGenericEventRunner(config,
                Assembly.GetAssembly(typeof(BeforeHandler)),
                Assembly.GetAssembly(typeof(DuringHandler)),
                Assembly.GetAssembly(typeof(AfterHandler))
            );

            //VERIFY
            services.Contains(new ServiceDescriptor(typeof(IBeforeSaveEventHandler<OrderCreatedEvent>),
                typeof(BeforeHandler),
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();
            services.Contains(new ServiceDescriptor(typeof(IDuringSaveEventHandler<NewBookEvent>),
                typeof(DuringHandler),
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();
            services.Contains(new ServiceDescriptor(typeof(IAfterSaveEventHandler<OrderReadyToDispatchEvent>),
                typeof(AfterHandler),
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();

            foreach (var log in logs)
            {
                _output.WriteLine(log);
            }

            config.NotUsingDuringSaveHandlers.ShouldBeFalse();
            config.NotUsingAfterSaveHandlers.ShouldBeFalse();
            services.Count.ShouldEqual(5);
        }
    }
}