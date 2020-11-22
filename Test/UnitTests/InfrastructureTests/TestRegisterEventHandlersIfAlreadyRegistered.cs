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
        public void TestRegisterEventHandlersOnlyHandlers()
        {
            //SETUP
            var services = new ServiceCollection();
            var config = new GenericEventRunnerConfig();

            //ATTEMPT
            var logs = services.RegisterGenericEventRunner(config,
                Assembly.GetAssembly(typeof(ExtraDispatchAfterHandler)),
                Assembly.GetAssembly(typeof(ExtraOrderCreatedHandler)),
                Assembly.GetAssembly(typeof(ExtraDuringEventHandler))
                );

            //VERIFY
            services.Contains(new ServiceDescriptor(typeof(IAfterSaveEventHandler<OrderReadyToDispatchEvent>),
                typeof(ExtraDispatchAfterHandler),
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();
            services.Contains(new ServiceDescriptor(typeof(IBeforeSaveEventHandler<OrderCreatedEvent>),
                typeof(ExtraOrderCreatedHandler),
                ServiceLifetime.Transient), new ServiceDescriptorIncludeLifeTimeCompare()).ShouldBeTrue();
            services.Contains(new ServiceDescriptor(typeof(IDuringSaveEventHandler<NewBookEvent>),
                typeof(ExtraDuringEventHandler),
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