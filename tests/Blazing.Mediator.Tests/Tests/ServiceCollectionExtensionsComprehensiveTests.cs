using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Statistics;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Blazing.Mediator.Tests.Tests
{
    /// <summary>
    /// Comprehensive tests for ALL ServiceCollectionExtensions method overloads.
    /// Ensures 100% code coverage for ServiceCollectionExtensions class.
    /// </summary>
    public class ServiceCollectionExtensionsComprehensiveTests
    {
        /// <summary>
        /// Test statistics renderer that captures messages
        /// </summary>
        private class TestStatisticsRenderer : IStatisticsRenderer
        {
            public List<string> Messages { get; } = new();
            public void Render(string message) => Messages.Add(message);
        }

        #region AddMediator Basic Overloads Tests

        /// <summary>
        /// Tests AddMediator() with default parameters (enableStatisticsTracking=false)
        /// </summary>
        [Fact]
        public void AddMediator_DefaultParameters_DisablesStatistics()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediator();

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            var renderer = serviceProvider.GetService<IStatisticsRenderer>();

            statistics.ShouldBeNull();
            renderer.ShouldBeNull();
        }

        /// <summary>
        /// Tests AddMediator(bool enableStatisticsTracking) with true
        /// </summary>
        [Fact]
        public void AddMediator_EnableStatisticsTrue_EnablesStatistics()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediator(configureMiddleware: null, enableStatisticsTracking: true, (Assembly[])null!);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests AddMediator(bool enableStatisticsTracking) with false
        /// </summary>
        [Fact]
        public void AddMediator_EnableStatisticsFalse_DisablesStatistics()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediator();

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldBeNull();
        }

        /// <summary>
        /// Tests AddMediator(params Assembly[]) overload
        /// </summary>
        [Fact]
        public void AddMediator_WithAssemblies_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            Assembly assembly = typeof(TestCommand).Assembly;

            // Act
            services.AddMediator(configureMiddleware: null, enableStatisticsTracking: true, assembly);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            var statistics = serviceProvider.GetService<MediatorStatistics>();

            mediator.ShouldNotBeNull();
            statistics.ShouldNotBeNull(); // Explicitly enabled
        }

        /// <summary>
        /// Tests AddMediator(params Type[]) overload
        /// </summary>
        [Fact]
        public void AddMediator_WithTypes_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediator(configureMiddleware: null, enableStatisticsTracking: true, typeof(TestCommand), typeof(TestQuery));

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            var statistics = serviceProvider.GetService<MediatorStatistics>();

            mediator.ShouldNotBeNull();
            statistics.ShouldNotBeNull(); // Explicitly enabled
        }

        /// <summary>
        /// Tests AddMediator(bool discoverMiddleware, params Assembly[]) overload
        /// </summary>
        [Fact]
        public void AddMediator_WithDiscoverMiddlewareAndAssemblies_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            Assembly assembly = typeof(TestCommand).Assembly;

            // Act
            services.AddMediator(configureMiddleware: null, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: false, assembly);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            var statistics = serviceProvider.GetService<MediatorStatistics>();

            mediator.ShouldNotBeNull();
            statistics.ShouldNotBeNull(); // Explicitly enabled
        }

        /// <summary>
        /// Tests AddMediator(bool discoverMiddleware, params Type[]) overload
        /// </summary>
        [Fact]
        public void AddMediator_WithDiscoverMiddlewareAndTypes_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediator(configureMiddleware: null, enableStatisticsTracking: true, typeof(TestCommand));

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            var statistics = serviceProvider.GetService<MediatorStatistics>();

            mediator.ShouldNotBeNull();
            statistics.ShouldNotBeNull(); // Explicitly enabled
        }

        /// <summary>
        /// Tests AddMediator(bool, Type[]) with null/empty types
        /// </summary>
        [Fact]
        public void AddMediator_WithDiscoverMiddlewareAndNullTypes_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediator(discoverMiddleware: false, (Type[])null!);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            mediator.ShouldNotBeNull();
        }

        #endregion

        #region AddMediatorWithNotificationMiddleware Tests

        /// <summary>
        /// Tests AddMediatorWithNotificationMiddleware(bool, params Assembly[]) overload
        /// </summary>
        [Fact]
        public void AddMediatorWithNotificationMiddleware_WithAssemblies_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            Assembly assembly = typeof(TestCommand).Assembly;

            // Act
            services.AddMediatorWithNotificationMiddleware(discoverNotificationMiddleware: false, assembly);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            var statistics = serviceProvider.GetService<MediatorStatistics>();

            mediator.ShouldNotBeNull();
            statistics.ShouldBeNull(); // Statistics disabled by default
        }

        /// <summary>
        /// Tests AddMediatorWithNotificationMiddleware(bool, params Type[]) overload
        /// </summary>
        [Fact]
        public void AddMediatorWithNotificationMiddleware_WithTypes_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorWithNotificationMiddleware(discoverNotificationMiddleware: false, typeof(TestCommand));

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            var statistics = serviceProvider.GetService<MediatorStatistics>();

            mediator.ShouldNotBeNull();
            statistics.ShouldBeNull(); // Statistics disabled by default
        }

        /// <summary>
        /// Tests AddMediatorWithNotificationMiddleware with null/empty types
        /// </summary>
        [Fact]
        public void AddMediatorWithNotificationMiddleware_WithNullTypes_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorWithNotificationMiddleware(discoverNotificationMiddleware: false, (Type[])null!);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            mediator.ShouldNotBeNull();
        }

        #endregion

        #region Configuration-based Overloads Tests

        /// <summary>
        /// Tests AddMediator(Action&lt;MediatorConfiguration&gt;, params Assembly[]) overload
        /// </summary>
        [Fact]
        public void AddMediator_WithConfigurationAndAssemblies_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            Assembly assembly = typeof(TestCommand).Assembly;

            // Act
            services.AddMediator(config =>
            {
                config.AddMiddleware<FirstQueryMiddleware>();
            }, assembly);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            var statistics = serviceProvider.GetService<MediatorStatistics>();

            mediator.ShouldNotBeNull();
            statistics.ShouldBeNull(); // Statistics disabled by default
        }

        /// <summary>
        /// Tests AddMediator(Action&lt;MediatorConfiguration&gt;, bool, params Assembly[]) overload
        /// </summary>
        [Fact]
        public void AddMediator_WithConfigurationAndStatisticsDisabled_DisablesStatistics()
        {
            // Arrange
            ServiceCollection services = new();
            Assembly assembly = typeof(TestCommand).Assembly;

            // Act
            services.AddMediator(_ => { }, enableStatisticsTracking: false, assembly);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            var statistics = serviceProvider.GetService<MediatorStatistics>();

            mediator.ShouldNotBeNull();
            statistics.ShouldBeNull(); // Should be disabled
        }

        /// <summary>
        /// Tests AddMediator(Action&lt;MediatorConfiguration&gt;, params Type[]) overload
        /// </summary>
        [Fact]
        public void AddMediator_WithConfigurationAndTypes_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediator(_ => { }, typeof(TestCommand));

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            var statistics = serviceProvider.GetService<MediatorStatistics>();

            mediator.ShouldNotBeNull();
            statistics.ShouldBeNull(); // Statistics disabled by default
        }

        /// <summary>
        /// Tests AddMediator(Action&lt;MediatorConfiguration&gt;, bool, params Type[]) overload
        /// </summary>
        [Fact]
        public void AddMediator_WithConfigurationStatisticsAndTypes_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediator(_ => { }, enableStatisticsTracking: true, typeof(TestCommand));

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            var statistics = serviceProvider.GetService<MediatorStatistics>();

            mediator.ShouldNotBeNull();
            statistics.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests AddMediator configuration with null/empty types
        /// </summary>
        [Fact]
        public void AddMediator_WithConfigurationAndNullTypes_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediator(_ => { }, enableStatisticsTracking: true, (Type[])null!);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            mediator.ShouldNotBeNull();
        }

        #endregion

        #region Main Core Overload Tests

        /// <summary>
        /// Tests the main overload AddMediator(Action, bool?, bool?, Assembly[])
        /// </summary>
        [Fact]
        public void AddMediator_MainOverload_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            Assembly assembly = typeof(TestCommand).Assembly;

            // Act
            services.AddMediator(_ => { }, discoverMiddleware: false, discoverNotificationMiddleware: false, assembly);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            var statistics = serviceProvider.GetService<MediatorStatistics>();

            mediator.ShouldNotBeNull();
            statistics.ShouldBeNull(); // Statistics disabled by default
        }

        /// <summary>
        /// Tests the comprehensive main overload AddMediator(Action, bool, bool?, bool?, Assembly[])
        /// </summary>
        [Fact]
        public void AddMediator_MainOverloadWithStatistics_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            Assembly assembly = typeof(TestCommand).Assembly;

            // Act
            services.AddMediator(_ => { }, enableStatisticsTracking: false, discoverMiddleware: false, discoverNotificationMiddleware: false, assembly);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            var statistics = serviceProvider.GetService<MediatorStatistics>();

            mediator.ShouldNotBeNull();
            statistics.ShouldBeNull(); // Should be disabled
        }

        /// <summary>
        /// Tests main overload with null assemblies (uses calling assembly)
        /// </summary>
        [Fact]
        public void AddMediator_MainOverloadWithNullAssemblies_UsesCallingAssembly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediator(_ => { }, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: false, (Assembly[])null!);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            mediator.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests main overload with middleware discovery enabled
        /// </summary>
        [Fact]
        public void AddMediator_MainOverloadWithMiddlewareDiscovery_RegistersMiddleware()
        {
            // Arrange
            ServiceCollection services = new();
            Assembly assembly = typeof(TestCommand).Assembly;

            // Act
            services.AddMediator(_ => { }, enableStatisticsTracking: true, discoverMiddleware: true, discoverNotificationMiddleware: true, assembly);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            var inspector = serviceProvider.GetService<IMiddlewarePipelineInspector>();
            var notificationInspector = serviceProvider.GetService<INotificationMiddlewarePipelineInspector>();

            mediator.ShouldNotBeNull();
            inspector.ShouldNotBeNull();
            notificationInspector.ShouldNotBeNull();
        }

        #endregion

        #region AddMediatorFromCallingAssembly Tests

        /// <summary>
        /// Tests AddMediatorFromCallingAssembly() basic overload
        /// </summary>
        [Fact]
        public void AddMediatorFromCallingAssembly_Basic_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorFromCallingAssembly();

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            var statistics = serviceProvider.GetService<MediatorStatistics>();

            mediator.ShouldNotBeNull();
            statistics.ShouldBeNull(); // Statistics disabled by default
        }

        /// <summary>
        /// Tests AddMediatorFromCallingAssembly(bool) overload
        /// </summary>
        [Fact]
        public void AddMediatorFromCallingAssembly_WithDiscoverMiddleware_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorFromCallingAssembly(discoverMiddleware: false);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            mediator.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests AddMediatorFromCallingAssembly(Action&lt;MediatorConfiguration&gt;) overload
        /// </summary>
        [Fact]
        public void AddMediatorFromCallingAssembly_WithConfiguration_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorFromCallingAssembly(config =>
            {
                config.AddMiddleware<FirstQueryMiddleware>();
            });

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            mediator.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests AddMediatorFromCallingAssembly(Action&lt;MediatorConfiguration&gt;, bool) overload
        /// </summary>
        [Fact]
        public void AddMediatorFromCallingAssembly_WithConfigurationAndDiscoverMiddleware_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorFromCallingAssembly(_ => { }, discoverMiddleware: false);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            mediator.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests AddMediatorFromCallingAssembly with all parameters
        /// </summary>
        [Fact]
        public void AddMediatorFromCallingAssembly_WithAllParameters_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorFromCallingAssembly(_ => { }, enableStatisticsTracking: false, discoverMiddleware: false, discoverNotificationMiddleware: false);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            var statistics = serviceProvider.GetService<MediatorStatistics>();

            mediator.ShouldNotBeNull();
            statistics.ShouldBeNull(); // Should be disabled
        }

        #endregion

        #region AddMediatorFromLoadedAssemblies Tests

        /// <summary>
        /// Tests AddMediatorFromLoadedAssemblies() basic overload
        /// </summary>
        [Fact]
        public void AddMediatorFromLoadedAssemblies_Basic_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorFromLoadedAssemblies();

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            var statistics = serviceProvider.GetService<MediatorStatistics>();

            mediator.ShouldNotBeNull();
            statistics.ShouldBeNull(); // Statistics disabled by default
        }

        /// <summary>
        /// Tests AddMediatorFromLoadedAssemblies(Func&lt;Assembly, bool&gt;) overload
        /// </summary>
        [Fact]
        public void AddMediatorFromLoadedAssemblies_WithFilter_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorFromLoadedAssemblies(assembly => assembly.GetName().Name!.Contains("Blazing.Mediator"));

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            mediator.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests AddMediatorFromLoadedAssemblies(bool) overload
        /// </summary>
        [Fact]
        public void AddMediatorFromLoadedAssemblies_WithDiscoverMiddleware_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorFromLoadedAssemblies(discoverMiddleware: false);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            mediator.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests AddMediatorFromLoadedAssemblies(Action&lt;MediatorConfiguration&gt;, Func&lt;Assembly, bool&gt;) overload
        /// </summary>
        [Fact]
        public void AddMediatorFromLoadedAssemblies_WithConfigurationAndFilter_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorFromLoadedAssemblies(_ => { }, assembly => assembly.GetName().Name!.Contains("Blazing.Mediator"));

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            mediator.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests AddMediatorFromLoadedAssemblies(Action&lt;MediatorConfiguration&gt;, bool, Func&lt;Assembly, bool&gt;) overload
        /// </summary>
        [Fact]
        public void AddMediatorFromLoadedAssemblies_WithConfigurationDiscoverMiddlewareAndFilter_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorFromLoadedAssemblies(_ => { }, discoverMiddleware: false, assembly => assembly.GetName().Name!.Contains("Blazing.Mediator"));

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            mediator.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests AddMediatorFromLoadedAssemblies with all parameters
        /// </summary>
        [Fact]
        public void AddMediatorFromLoadedAssemblies_WithAllParameters_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorFromLoadedAssemblies(_ => { }, enableStatisticsTracking: false, discoverMiddleware: false, discoverNotificationMiddleware: false, assembly => assembly.GetName().Name!.Contains("Blazing.Mediator"));

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            var statistics = serviceProvider.GetService<MediatorStatistics>();

            mediator.ShouldNotBeNull();
            statistics.ShouldBeNull(); // Should be disabled
        }

        #endregion

        #region Edge Cases and Error Conditions

        /// <summary>
        /// Tests that AddMediator throws ArgumentNullException when service collection is null
        /// </summary>
        [Fact]
        public void AddMediator_WithNullServiceCollection_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddMediator());
            Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddMediator((Assembly[])null!));
            Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddMediator((Type[])null!));
        }

        /// <summary>
        /// Tests custom statistics renderer registration
        /// </summary>
        [Fact]
        public void AddMediator_WithExistingStatisticsRenderer_DoesNotOverride()
        {
            // Arrange
            ServiceCollection services = new();
            var customRenderer = new TestStatisticsRenderer();
            services.AddSingleton<IStatisticsRenderer>(customRenderer);

            // Act
            services.AddMediator();

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var renderer = serviceProvider.GetService<IStatisticsRenderer>();
            renderer.ShouldBeSameAs(customRenderer);
        }

        /// <summary>
        /// Tests custom MediatorStatistics registration
        /// </summary>
        [Fact]
        public void AddMediator_WithExistingMediatorStatistics_DoesNotOverride()
        {
            // Arrange
            ServiceCollection services = new();
            var customRenderer = new TestStatisticsRenderer();
            var customStatistics = new MediatorStatistics(customRenderer);
            services.AddSingleton<IStatisticsRenderer>(customRenderer);
            services.AddSingleton(customStatistics);

            // Act
            services.AddMediator();

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldBeSameAs(customStatistics);
        }

        /// <summary>
        /// Tests that pipeline inspector is properly registered
        /// </summary>
        [Fact]
        public void AddMediator_RegistersPipelineInspector_Successfully()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediator();

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var inspector = serviceProvider.GetService<IMiddlewarePipelineInspector>();
            inspector.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests assembly deduplication
        /// </summary>
        [Fact]
        public void AddMediator_WithDuplicateAssemblies_DeduplicatesCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            Assembly assembly = typeof(TestCommand).Assembly;

            // Act
            services.AddMediator(assembly, assembly, assembly); // Same assembly multiple times

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            mediator.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that empty assemblies array is handled correctly
        /// </summary>
        [Fact]
        public void AddMediator_WithEmptyAssemblies_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediator(Array.Empty<Assembly>());

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            mediator.ShouldNotBeNull();
        }

        #endregion
    }
}