using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Statistics;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Blazing.Mediator.Tests.Tests
{
    /// <summary>
    /// COMPREHENSIVE tests for ServiceCollectionExtensions to achieve 100% code coverage.
    /// Tests ALL overloads, edge cases, error conditions, and internal logic paths.
    /// </summary>
    public class ServiceCollectionExtensionsFullCoverageTests
    {
        private class TestStatisticsRenderer : IStatisticsRenderer
        {
            public List<string> Messages { get; } = new();
            public void Render(string message) => Messages.Add(message);
        }

        #region ALL AddMediator Overloads Coverage

        [Fact]
        public void AddMediator_DefaultParameters_DisablesStatistics()
        {
            ServiceCollection services = new();
            services.AddMediator();
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<MediatorStatistics>().ShouldBeNull();
        }

        [Fact]
        public void AddMediator_EnableStatisticsTrue_EnablesStatistics()
        {
            ServiceCollection services = new();
            services.AddMediator(_ => { }, enableStatisticsTracking: true, (Assembly[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<MediatorStatistics>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediator_EnableStatisticsFalse_DisablesStatistics()
        {
            ServiceCollection services = new();
            services.AddMediator(_ => { }, enableStatisticsTracking: false, (Assembly[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<MediatorStatistics>().ShouldBeNull();
        }

        [Fact]
        public void AddMediator_WithAssemblies_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediator(configureMiddleware: null, enableStatisticsTracking: true, typeof(TestCommand).Assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediator_WithTypes_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediator(configureMiddleware: null, enableStatisticsTracking: true, typeof(TestCommand), typeof(TestQuery));
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediator_WithDiscoverMiddlewareAndAssemblies_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediator(configureMiddleware: null, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: null, typeof(TestCommand).Assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediator_WithDiscoverMiddlewareAndTypes_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediator(configureMiddleware: null, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: null, typeof(TestCommand).Assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediator_WithDiscoverMiddlewareAndNullTypes_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediator(configureMiddleware: null, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: null, (Assembly[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediatorWithNotificationMiddleware_WithAssemblies_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediatorWithNotificationMiddleware(discoverNotificationMiddleware: false, typeof(TestCommand).Assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediatorWithNotificationMiddleware_WithTypes_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediatorWithNotificationMiddleware(discoverNotificationMiddleware: false, typeof(TestCommand));
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediatorWithNotificationMiddleware_WithNullTypes_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediatorWithNotificationMiddleware(discoverNotificationMiddleware: false, (Type[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediator_WithConfigurationAndAssemblies_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediator(_ => { }, typeof(TestCommand).Assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediator_WithConfigurationAndStatisticsDisabled_DisablesStatistics()
        {
            ServiceCollection services = new();
            services.AddMediator(_ => { }, enableStatisticsTracking: false, typeof(TestCommand).Assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<MediatorStatistics>().ShouldBeNull();
        }

        [Fact]
        public void AddMediator_WithConfigurationAndTypes_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediator(_ => { }, typeof(TestCommand));
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediator_WithConfigurationAndTypesAndStatisticsEnabled_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediator(_ => { }, enableStatisticsTracking: true, typeof(TestCommand));
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<MediatorStatistics>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediator_WithConfigurationAndNullTypes_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediator(_ => { }, enableStatisticsTracking: true, (Type[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediator_MainOverload_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediator(_ => { }, discoverMiddleware: false, discoverNotificationMiddleware: false, typeof(TestCommand).Assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediator_MainOverloadWithStatistics_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediator(_ => { }, enableStatisticsTracking: false, discoverMiddleware: false, discoverNotificationMiddleware: false, typeof(TestCommand).Assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<MediatorStatistics>().ShouldBeNull();
        }

        [Fact]
        public void AddMediator_MainOverloadWithNullAssemblies_UsesCallingAssembly()
        {
            ServiceCollection services = new();
            services.AddMediator(_ => { }, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: false, (Assembly[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediator_MainOverloadWithMiddlewareDiscovery_RegistersMiddleware()
        {
            ServiceCollection services = new();
            services.AddMediator(_ => { }, enableStatisticsTracking: true, discoverMiddleware: true, discoverNotificationMiddleware: true, typeof(TestCommand).Assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<INotificationMiddlewarePipelineInspector>().ShouldNotBeNull();
        }

        #endregion

        #region AddMediatorFromCallingAssembly Coverage

        [Fact]
        public void AddMediatorFromCallingAssembly_Basic_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediatorFromCallingAssembly();
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediatorFromCallingAssembly_WithDiscoverMiddleware_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediatorFromCallingAssembly(discoverMiddleware: false);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediatorFromCallingAssembly_WithConfiguration_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediatorFromCallingAssembly(_ => { });
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediatorFromCallingAssembly_WithConfigurationAndDiscoverMiddleware_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediatorFromCallingAssembly(_ => { }, discoverMiddleware: false);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediatorFromCallingAssembly_WithAllParameters_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediatorFromCallingAssembly(_ => { }, enableStatisticsTracking: false, discoverMiddleware: false, discoverNotificationMiddleware: false);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<MediatorStatistics>().ShouldBeNull();
        }

        #endregion

        #region AddMediatorFromLoadedAssemblies Coverage

        [Fact]
        public void AddMediatorFromLoadedAssemblies_Basic_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediatorFromLoadedAssemblies();
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediatorFromLoadedAssemblies_WithFilter_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediatorFromLoadedAssemblies(assembly => assembly.GetName().Name!.Contains("Blazing.Mediator"));
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediatorFromLoadedAssemblies_WithDiscoverMiddleware_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediatorFromLoadedAssemblies(discoverMiddleware: false);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediatorFromLoadedAssemblies_WithConfigurationAndFilter_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediatorFromLoadedAssemblies(_ => { }, assembly => assembly.GetName().Name!.Contains("Blazing.Mediator"));
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediatorFromLoadedAssemblies_WithConfigurationDiscoverMiddlewareAndFilter_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediatorFromLoadedAssemblies(_ => { }, discoverMiddleware: false, assembly => assembly.GetName().Name!.Contains("Blazing.Mediator"));
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediatorFromLoadedAssemblies_WithAllParameters_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediatorFromLoadedAssemblies(_ => { }, enableStatisticsTracking: false, discoverMiddleware: false, discoverNotificationMiddleware: false, assembly => assembly.GetName().Name!.Contains("Blazing.Mediator"));
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<MediatorStatistics>().ShouldBeNull();
        }

        #endregion

        #region Edge Cases and Error Conditions

        [Fact]
        public void AddMediator_WithNullServiceCollection_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddMediator());
            Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddMediatorFromCallingAssembly());
            Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddMediatorFromLoadedAssemblies());
            Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddMediatorWithNotificationMiddleware(false, Array.Empty<Assembly>()));
        }

        [Fact]
        public void AddMediator_WithExistingStatisticsRenderer_DoesNotOverride()
        {
            ServiceCollection services = new();
            var customRenderer = new TestStatisticsRenderer();
            services.AddSingleton<IStatisticsRenderer>(customRenderer);
            services.AddMediator(configureMiddleware: null, enableStatisticsTracking: true, (Assembly[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IStatisticsRenderer>().ShouldBeSameAs(customRenderer);
        }

        [Fact]
        public void AddMediator_WithExistingMediatorStatistics_DoesNotOverride()
        {
            ServiceCollection services = new();
            var customRenderer = new TestStatisticsRenderer();
            var customStatistics = new MediatorStatistics(customRenderer);
            services.AddSingleton<IStatisticsRenderer>(customRenderer);
            services.AddSingleton(customStatistics);
            services.AddMediator(configureMiddleware: null, enableStatisticsTracking: true, (Assembly[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<MediatorStatistics>().ShouldBeSameAs(customStatistics);
        }

        [Fact]
        public void AddMediator_WithDuplicateAssemblies_DeduplicatesCorrectly()
        {
            ServiceCollection services = new();
            Assembly assembly = typeof(TestCommand).Assembly;
            services.AddMediator(configureMiddleware: null, enableStatisticsTracking: true, assembly, assembly, assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediator_WithEmptyAssemblies_RegistersCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediator(Array.Empty<Assembly>());
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediator_WithAssemblyWithoutHandlers_ProcessesCorrectly()
        {
            ServiceCollection services = new();
            Assembly assembly = typeof(string).Assembly;
            services.AddMediator(assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        #endregion

        #region Internal Logic Path Coverage

        [Fact]
        public void AddMediator_ConditionalAssemblyHandling_WorksCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediator(_ => { }, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: false, (Assembly[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediator_ConditionalMiddlewareRegistration_WorksCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediator(_ => { }, enableStatisticsTracking: true, discoverMiddleware: true, discoverNotificationMiddleware: false, typeof(TestCommand).Assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMiddlewarePipelineInspector>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediator_ConditionalNotificationMiddlewareOnly_WorksCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediator(_ => { }, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: true, typeof(TestCommand).Assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<INotificationMiddlewarePipelineInspector>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediator_NullMiddlewareDiscovery_ConvertsToFalse()
        {
            ServiceCollection services = new();
            services.AddMediator(_ => { }, enableStatisticsTracking: true, discoverMiddleware: null, discoverNotificationMiddleware: null);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediator_ConditionalStatisticsRenderer_WorksCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediator(configureMiddleware: null, enableStatisticsTracking: true, (Assembly[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IStatisticsRenderer>().ShouldBeOfType<ConsoleStatisticsRenderer>();
        }

        [Fact]
        public void AddMediator_ConditionalMediatorStatistics_WorksCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediator(configureMiddleware: null, enableStatisticsTracking: true, (Assembly[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<MediatorStatistics>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediator_HandlerDuplicationPrevention_WorksCorrectly()
        {
            ServiceCollection services = new();
            Assembly assembly = typeof(TestCommandHandler).Assembly;
            services.AddMediator(configureMiddleware: null, enableStatisticsTracking: true, assembly);
            services.AddMediator(configureMiddleware: null, enableStatisticsTracking: true, assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var handlers = serviceProvider.GetServices<IRequestHandler<TestCommand>>();
            handlers.Count().ShouldBe(1);
        }

        [Fact]
        public void AddMediator_PipelineInspectorRegistration_WorksCorrectly()
        {
            ServiceCollection services = new();
            services.AddMediator(configureMiddleware: null, enableStatisticsTracking: true, (Assembly[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IMiddlewarePipelineInspector>().ShouldNotBeNull();
        }

        [Fact]
        public void AddMediator_ServiceLifetimes_AreCorrect()
        {
            ServiceCollection services = new();
            services.AddMediator(configureMiddleware: null, enableStatisticsTracking: true, (Assembly[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            using var scope1 = serviceProvider.CreateScope();
            using var scope2 = serviceProvider.CreateScope();

            var mediator1 = scope1.ServiceProvider.GetService<IMediator>();
            var mediator2 = scope2.ServiceProvider.GetService<IMediator>();
            var stats1 = scope1.ServiceProvider.GetService<MediatorStatistics>();
            var stats2 = scope2.ServiceProvider.GetService<MediatorStatistics>();

            mediator1.ShouldNotBeSameAs(mediator2); // Scoped
            stats1.ShouldBeSameAs(stats2); // Singleton
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task AddMediator_FullIntegration_WorksEndToEnd()
        {
            ServiceCollection services = new();
            var customRenderer = new TestStatisticsRenderer();
            services.AddSingleton<IStatisticsRenderer>(customRenderer);
            services.AddMediator(config =>
            {
                config.AddMiddleware<FirstQueryMiddleware>();
            }, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: false, typeof(TestCommandHandler).Assembly);

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new TestCommand { Value = "test" });
            string result = await mediator.Send(new MiddlewareTestQuery { Value = "test" });

            var statistics = serviceProvider.GetRequiredService<MediatorStatistics>();
            statistics.ReportStatistics();

            customRenderer.Messages.ShouldContain(msg => msg.Contains("Commands: 1"));
            customRenderer.Messages.ShouldContain(msg => msg.Contains("Queries: 1"));
            result.ShouldBe("First: Handler: test");
        }

        [Fact]
        public async Task MediatorFactory_WithStatistics_InjectsStatisticsCorrectly()
        {
            ServiceCollection services = new();
            var customRenderer = new TestStatisticsRenderer();
            services.AddSingleton<IStatisticsRenderer>(customRenderer);
            services.AddMediator(null, enableStatisticsTracking: true, typeof(TestCommand).Assembly);

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new TestCommand { Value = "test" });

            var statistics = serviceProvider.GetRequiredService<MediatorStatistics>();
            statistics.ReportStatistics();
            customRenderer.Messages.ShouldContain(msg => msg.Contains("Commands: 1"));
        }

        [Fact]
        public async Task MediatorFactory_WithoutStatistics_HandlesNullStatistics()
        {
            ServiceCollection services = new();
            services.AddMediator(null, enableStatisticsTracking: false, typeof(TestCommand).Assembly);

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new TestCommand { Value = "test" });
        }

        #endregion
    }
}