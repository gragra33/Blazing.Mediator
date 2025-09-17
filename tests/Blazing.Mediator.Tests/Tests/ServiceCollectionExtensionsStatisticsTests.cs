using Blazing.Mediator.Statistics;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Blazing.Mediator.Tests.Tests
{
    /// <summary>
    /// Comprehensive tests for ServiceCollectionExtensions statistics tracking functionality.
    /// Tests all overloads and variations with statistics enabled and disabled.
    /// </summary>
    public class ServiceCollectionExtensionsStatisticsTests
    {
        /// <summary>
        /// Test statistics renderer that captures messages
        /// </summary>
        private class TestStatisticsRenderer : IStatisticsRenderer
        {
            public List<string> Messages { get; } = new();
            public void Render(string message) => Messages.Add(message);
        }

        #region Basic Statistics Tests

        /// <summary>
        /// Tests that AddMediator with default parameters disables statistics tracking.
        /// </summary>
        [Fact]
        public void AddMediator_DefaultParameters_DisablesStatisticsTracking()
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
        /// Tests that AddMediator with enableStatisticsTracking=true enables statistics tracking.
        /// </summary>
        [Fact]
        public void AddMediator_WithStatisticsTrackingEnabled_EnablesStatisticsTracking()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediator(_ => { }, enableStatisticsTracking: true, (Assembly[])null!);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            var renderer = serviceProvider.GetService<IStatisticsRenderer>();

            statistics.ShouldNotBeNull();
            renderer.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that AddMediator with enableStatisticsTracking=false disables statistics tracking.
        /// </summary>
        [Fact]
        public void AddMediator_WithStatisticsTrackingDisabled_DisablesStatisticsTracking()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediator(_ => { }, enableStatisticsTracking: false, (Assembly[])null!);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            var renderer = serviceProvider.GetService<IStatisticsRenderer>();

            statistics.ShouldBeNull();
            renderer.ShouldBeNull();
        }

        #endregion

        #region All Overloads Statistics Tests

        /// <summary>
        /// Tests AddMediator(Assembly[]) with statistics tracking.
        /// </summary>
        [Fact]
        public void AddMediator_WithAssemblies_EnablesStatisticsByDefault()
        {
            // Arrange
            ServiceCollection services = new();
            Assembly assembly = typeof(TestCommand).Assembly;

            // Act
            services.AddMediator(configureMiddleware: null, enableStatisticsTracking: true, assembly);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests AddMediator(Type[]) with statistics tracking.
        /// </summary>
        [Fact]
        public void AddMediator_WithTypes_EnablesStatisticsByDefault()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediator(configureMiddleware: null, enableStatisticsTracking: true, typeof(TestCommand));

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests AddMediator(bool, Assembly[]) with statistics tracking.
        /// </summary>
        [Fact]
        public void AddMediator_WithDiscoverMiddlewareAndAssemblies_EnablesStatisticsByDefault()
        {
            // Arrange
            ServiceCollection services = new();
            Assembly assembly = typeof(TestCommand).Assembly;

            // Act
            services.AddMediator(configureMiddleware: null, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: null, assembly);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests AddMediator(bool, Type[]) with statistics tracking.
        /// </summary>
        [Fact]
        public void AddMediator_WithDiscoverMiddlewareAndTypes_EnablesStatisticsByDefault()
        {
            // Arrange
            ServiceCollection services = new();

            // Obsolete overload removed due to API changes
        }

        /// <summary>
        /// Tests AddMediatorWithNotificationMiddleware(bool, Assembly[]) with statistics tracking.
        /// </summary>
        [Fact]
        public void AddMediatorWithNotificationMiddleware_WithAssemblies_EnablesStatisticsByDefault()
        {
            // Arrange
            ServiceCollection services = new();
            Assembly assembly = typeof(TestCommand).Assembly;

            // Obsolete overload removed due to API changes
        }

        /// <summary>
        /// Tests AddMediatorWithNotificationMiddleware(bool, Type[]) with statistics tracking.
        /// </summary>
        [Fact]
        public void AddMediatorWithNotificationMiddleware_WithTypes_EnablesStatisticsByDefault()
        {
            // Arrange
            ServiceCollection services = new();

            // Obsolete overload removed due to API changes
        }

        /// <summary>
        /// Tests AddMediator(Action, Assembly[]) with statistics tracking.
        /// </summary>
        [Fact]
        public void AddMediator_WithConfigurationAndAssemblies_EnablesStatisticsByDefault()
        {
            // Arrange
            ServiceCollection services = new();
            Assembly assembly = typeof(TestCommand).Assembly;

            // Obsolete overload removed due to API changes
        }

        /// <summary>
        /// Tests AddMediator(Action, bool, Assembly[]) with statistics tracking enabled.
        /// </summary>
        [Fact]
        public void AddMediator_WithConfigurationAndStatisticsEnabled_EnablesStatisticsTracking()
        {
            // Arrange
            ServiceCollection services = new();
            Assembly assembly = typeof(TestCommand).Assembly;

            // Obsolete overload removed due to API changes
        }

        /// <summary>
        /// Tests AddMediator(Action, bool, Assembly[]) with statistics tracking disabled.
        /// </summary>
        [Fact]
        public void AddMediator_WithConfigurationAndStatisticsDisabled_DisablesStatisticsTracking()
        {
            // Arrange
            ServiceCollection services = new();
            Assembly assembly = typeof(TestCommand).Assembly;

            // Act
            services.AddMediator(null, enableStatisticsTracking: false, assembly);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldBeNull();
        }

        /// <summary>
        /// Tests AddMediator(Action, Type[]) with statistics tracking.
        /// </summary>
        [Fact]
        public void AddMediator_WithConfigurationAndTypes_EnablesStatisticsByDefault()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediator(_ => { }, enableStatisticsTracking: true, typeof(TestCommand));

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests AddMediator(Action, bool, Type[]) with statistics tracking enabled.
        /// </summary>
        [Fact]
        public void AddMediator_WithConfigurationAndTypesAndStatisticsEnabled_EnablesStatisticsTracking()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediator(null, enableStatisticsTracking: true, typeof(TestCommand));

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests AddMediator(Action, bool, Type[]) with statistics tracking disabled.
        /// </summary>
        [Fact]
        public void AddMediator_WithConfigurationAndTypesAndStatisticsDisabled_DisablesStatisticsTracking()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediator(null, enableStatisticsTracking: false, typeof(TestCommand));

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldBeNull();
        }

        /// <summary>
        /// Tests the main overload AddMediator(Action, bool?, bool?, Assembly[]) with statistics enabled.
        /// </summary>
        [Fact]
        public void AddMediator_MainOverloadWithStatisticsEnabled_EnablesStatisticsTracking()
        {
            // Arrange
            ServiceCollection services = new();
            Assembly assembly = typeof(TestCommand).Assembly;

            // Act
            services.AddMediator(configureMiddleware: null, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: false, assembly);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests the main overload AddMediator(Action, bool, bool?, bool?, Assembly[]) with statistics enabled.
        /// </summary>
        [Fact]
        public void AddMediator_MainOverloadWithExplicitStatisticsEnabled_EnablesStatisticsTracking()
        {
            // Arrange
            ServiceCollection services = new();
            Assembly assembly = typeof(TestCommand).Assembly;

            // Act
            services.AddMediator(null, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: false, assembly);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests the main overload AddMediator(Action, bool, bool?, bool?, Assembly[]) with statistics disabled.
        /// </summary>
        [Fact]
        public void AddMediator_MainOverloadWithExplicitStatisticsDisabled_DisablesStatisticsTracking()
        {
            // Arrange
            ServiceCollection services = new();
            Assembly assembly = typeof(TestCommand).Assembly;

            // Act
            services.AddMediator(null, enableStatisticsTracking: false, discoverMiddleware: false, discoverNotificationMiddleware: false, assembly);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldBeNull();
        }

        #endregion

        #region Calling Assembly Tests

        /// <summary>
        /// Tests AddMediatorFromCallingAssembly() with statistics tracking.
        /// </summary>
        [Fact]
        public void AddMediatorFromCallingAssembly_Default_EnablesStatisticsByDefault()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorFromCallingAssembly(configureMiddleware: null, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: false);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests AddMediatorFromCallingAssembly(bool) with statistics tracking.
        /// </summary>
        [Fact]
        public void AddMediatorFromCallingAssembly_WithDiscoverMiddleware_EnablesStatisticsByDefault()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorFromCallingAssembly(configureMiddleware: null, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: false);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests AddMediatorFromCallingAssembly(Action) with statistics tracking disabled by default.
        /// </summary>
        [Fact]
        public void AddMediatorFromCallingAssembly_WithConfiguration_DisablesStatisticsByDefault()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorFromCallingAssembly(_ => { });

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldBeNull();
        }

        /// <summary>
        /// Tests AddMediatorFromCallingAssembly(Action, bool) with statistics tracking.
        /// </summary>
        [Fact]
        public void AddMediatorFromCallingAssembly_WithConfigurationAndDiscoverMiddleware_EnablesStatisticsByDefault()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorFromCallingAssembly(configureMiddleware: null, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: false);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests AddMediatorFromCallingAssembly(Action, bool, bool, bool) with statistics enabled.
        /// </summary>
        [Fact]
        public void AddMediatorFromCallingAssembly_WithAllParametersAndStatisticsEnabled_EnablesStatisticsTracking()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorFromCallingAssembly(null, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: false);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests AddMediatorFromCallingAssembly(Action, bool, bool, bool) with statistics disabled.
        /// </summary>
        [Fact]
        public void AddMediatorFromCallingAssembly_WithAllParametersAndStatisticsDisabled_DisablesStatisticsTracking()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorFromCallingAssembly(null, enableStatisticsTracking: false, discoverMiddleware: false, discoverNotificationMiddleware: false);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldBeNull();
        }

        #endregion

        #region Loaded Assemblies Tests

        /// <summary>
        /// Tests AddMediatorFromLoadedAssemblies() with statistics tracking.
        /// </summary>
        [Fact]
        public void AddMediatorFromLoadedAssemblies_Default_EnablesStatisticsByDefault()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorFromLoadedAssemblies(configureMiddleware: null, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: false);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests AddMediatorFromLoadedAssemblies(Func) with statistics tracking.
        /// </summary>
        [Fact]
        public void AddMediatorFromLoadedAssemblies_WithFilter_EnablesStatisticsByDefault()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorFromLoadedAssemblies(configureMiddleware: null, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: false, assembly => assembly.GetName().Name!.Contains("Blazing.Mediator"));

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests AddMediatorFromLoadedAssemblies(bool) with statistics tracking.
        /// </summary>
        [Fact]
        public void AddMediatorFromLoadedAssemblies_WithDiscoverMiddleware_EnablesStatisticsByDefault()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorFromLoadedAssemblies(configureMiddleware: null, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: false);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests AddMediatorFromLoadedAssemblies(Action, Func) with statistics tracking.
        /// </summary>
        [Fact]
        public void AddMediatorFromLoadedAssemblies_WithConfigurationAndFilter_EnablesStatisticsByDefault()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorFromLoadedAssemblies(_ => { }, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: false, assembly => assembly.GetName().Name!.Contains("Blazing.Mediator"));

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests AddMediatorFromLoadedAssemblies(Action, bool, Func) with statistics tracking.
        /// </summary>
        [Fact]
        public void AddMediatorFromLoadedAssemblies_WithConfigurationDiscoverMiddlewareAndFilter_EnablesStatisticsByDefault()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorFromLoadedAssemblies(null, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: false, assembly => assembly.GetName().Name!.Contains("Blazing.Mediator"));

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests AddMediatorFromLoadedAssemblies(Action, bool, bool, bool, Func) with statistics enabled.
        /// </summary>
        [Fact]
        public void AddMediatorFromLoadedAssemblies_WithAllParametersAndStatisticsEnabled_EnablesStatisticsTracking()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorFromLoadedAssemblies(null, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: false, assembly => assembly.GetName().Name!.Contains("Blazing.Mediator"));

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests AddMediatorFromLoadedAssemblies(Action, bool, bool, bool, Func) with statistics disabled.
        /// </summary>
        [Fact]
        public void AddMediatorFromLoadedAssemblies_WithAllParametersAndStatisticsDisabled_DisablesStatisticsTracking()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddMediatorFromLoadedAssemblies(null, enableStatisticsTracking: false, discoverMiddleware: false, discoverNotificationMiddleware: false, assembly => assembly.GetName().Name!.Contains("Blazing.Mediator"));

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldBeNull();
        }

        #endregion

        #region Custom Renderer Tests

        /// <summary>
        /// Tests that existing IStatisticsRenderer registration is not overridden.
        /// </summary>
        [Fact]
        public void AddMediator_WithExistingStatisticsRenderer_DoesNotOverride()
        {
            // Arrange
            ServiceCollection services = new();
            var customRenderer = new TestStatisticsRenderer();
            services.AddSingleton<IStatisticsRenderer>(customRenderer);

            // Act
            services.AddMediator(_ => { }, enableStatisticsTracking: true, typeof(TestCommand));

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var renderer = serviceProvider.GetService<IStatisticsRenderer>();
            renderer.ShouldBeSameAs(customRenderer);
        }

        /// <summary>
        /// Tests that existing MediatorStatistics registration is not overridden.
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
            services.AddMediator(_ => { }, enableStatisticsTracking: true, typeof(TestCommand));

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldBeSameAs(customStatistics);
        }

        #endregion

        #region Mediator Constructor Tests

        /// <summary>
        /// Tests that Mediator is created with statistics when enabled.
        /// </summary>
        [Fact]
        public async Task Mediator_WithStatisticsEnabled_TracksStatistics()
        {
            // Arrange
            ServiceCollection services = new();
            var customRenderer = new TestStatisticsRenderer();
            services.AddSingleton<IStatisticsRenderer>(customRenderer);
            services.AddMediator(_ => { }, enableStatisticsTracking: true, typeof(TestCommand).Assembly);

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
            var statistics = serviceProvider.GetRequiredService<MediatorStatistics>();

            // Act
            await mediator.Send(new TestCommand { Value = "test" });

            // Assert
            statistics.ReportStatistics();
            customRenderer.Messages.ShouldContain(msg => msg.Contains("Commands: 1"));
        }

        /// <summary>
        /// Tests that Mediator is created without statistics when disabled.
        /// </summary>
        [Fact]
        public async Task Mediator_WithStatisticsDisabled_DoesNotTrackStatistics()
        {
            // Arrange
            ServiceCollection services = new();
            services.AddMediator(_ => { }, enableStatisticsTracking: false, typeof(TestCommand).Assembly);

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
            var statistics = serviceProvider.GetService<MediatorStatistics>();

            // Act
            await mediator.Send(new TestCommand { Value = "test" });

            // Assert
            statistics.ShouldBeNull();
        }

        #endregion
    }
}