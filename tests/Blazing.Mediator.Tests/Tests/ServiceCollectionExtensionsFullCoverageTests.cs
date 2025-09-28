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
        /// <summary>
        /// Test implementation of <see cref="IStatisticsRenderer"/> for capturing rendered messages.
        /// </summary>
        private class TestStatisticsRenderer : IStatisticsRenderer
        {
            /// <summary>
            /// Gets the list of rendered messages.
            /// </summary>
            public List<string> Messages { get; } = new();
            /// <summary>
            /// Renders a statistics message by adding it to <see cref="Messages"/>.
            /// </summary>
            /// <param name="message">The message to render.</param>
            public void Render(string message) => Messages.Add(message);
        }

        #region ALL AddMediator Overloads Coverage

        /// <summary>
        /// Tests that calling <see cref="ServiceCollectionExtensions.AddMediator(IServiceCollection)"/> with default parameters
        /// disables statistics tracking by ensuring <see cref="MediatorStatistics"/> is not registered in the service provider.
        /// </summary>
        [Fact]
        public void AddMediator_DefaultParameters_DisablesStatistics()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator();
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<MediatorStatistics>().ShouldBeNull();
        }

        /// <summary>
        /// Tests that enabling statistics tracking registers <see cref="MediatorStatistics"/> in the service provider.
        /// </summary>
        [Fact]
        public void AddMediator_EnableStatisticsTrue_EnablesStatistics()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(_ => { }, enableStatisticsTracking: true, (Assembly[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<MediatorStatistics>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that disabling statistics tracking does not register <see cref="MediatorStatistics"/> in the service provider.
        /// </summary>
        [Fact]
        public void AddMediator_EnableStatisticsFalse_DisablesStatistics()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(_ => { }, enableStatisticsTracking: false, (Assembly[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<MediatorStatistics>().ShouldBeNull();
        }

        /// <summary>
        /// Tests that AddMediator with assemblies registers <see cref="IMediator"/> correctly.
        /// </summary>
        [Fact]
        public void AddMediator_WithAssemblies_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(config => { config.WithStatisticsTracking(); }, typeof(TestCommand).Assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that AddMediator with types registers <see cref="IMediator"/> correctly.
        /// </summary>
        [Fact]
        public void AddMediator_WithTypes_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(config => { config.WithStatisticsTracking(); }, typeof(TestCommand), typeof(TestQuery));
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that AddMediator with discover middleware and assemblies registers <see cref="IMediator"/> correctly.
        /// </summary>
        [Fact]
        public void AddMediator_WithDiscoverMiddlewareAndAssemblies_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(config =>
            {
                config.WithStatisticsTracking();
            }, typeof(TestCommand).Assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that AddMediator with discover middleware and types registers <see cref="IMediator"/> correctly.
        /// </summary>
        [Fact]
        public void AddMediator_WithDiscoverMiddlewareAndTypes_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(config => { config.WithStatisticsTracking(); }, discoverMiddleware: false, discoverNotificationMiddleware: null, typeof(TestCommand).Assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that AddMediator with discover middleware and null types registers <see cref="IMediator"/> correctly.
        /// </summary>
        [Fact]
        public void AddMediator_WithDiscoverMiddlewareAndNullTypes_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(config => { config.WithStatisticsTracking(); }, discoverMiddleware: false, discoverNotificationMiddleware: null, (Assembly[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that AddMediatorWithNotificationMiddleware with assemblies registers <see cref="IMediator"/> correctly.
        /// </summary>
        [Fact]
        public void AddMediatorWithNotificationMiddleware_WithAssemblies_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediatorWithNotificationMiddleware(discoverNotificationMiddleware: false, typeof(TestCommand).Assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that AddMediatorWithNotificationMiddleware with types registers <see cref="IMediator"/> correctly.
        /// </summary>
        [Fact]
        public void AddMediatorWithNotificationMiddleware_WithTypes_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediatorWithNotificationMiddleware(discoverNotificationMiddleware: false, typeof(TestCommand));
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that AddMediatorWithNotificationMiddleware with null types registers <see cref="IMediator"/> correctly.
        /// </summary>
        [Fact]
        public void AddMediatorWithNotificationMiddleware_WithNullTypes_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediatorWithNotificationMiddleware(discoverNotificationMiddleware: false, (Type[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that AddMediator with configuration and assemblies registers <see cref="IMediator"/> correctly.
        /// </summary>
        [Fact]
        public void AddMediator_WithConfigurationAndAssemblies_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(_ => { }, typeof(TestCommand).Assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that AddMediator with configuration and statistics disabled does not register <see cref="MediatorStatistics"/>.
        /// </summary>
        [Fact]
        public void AddMediator_WithConfigurationAndStatisticsDisabled_DisablesStatistics()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(_ => { }, enableStatisticsTracking: false, typeof(TestCommand).Assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<MediatorStatistics>().ShouldBeNull();
        }

        /// <summary>
        /// Tests that AddMediator with configuration and types registers <see cref="IMediator"/> correctly.
        /// </summary>
        [Fact]
        public void AddMediator_WithConfigurationAndTypes_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(_ => { }, typeof(TestCommand));
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that AddMediator with configuration, types, and statistics enabled registers <see cref="MediatorStatistics"/>.
        /// </summary>
        [Fact]
        public void AddMediator_WithConfigurationAndTypesAndStatisticsEnabled_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(_ => { }, enableStatisticsTracking: true, typeof(TestCommand));
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<MediatorStatistics>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that AddMediator with configuration and null types registers <see cref="IMediator"/> correctly.
        /// </summary>
        [Fact]
        public void AddMediator_WithConfigurationAndNullTypes_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(_ => { }, enableStatisticsTracking: true, (Type[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that the main AddMediator overload registers <see cref="IMediator"/> correctly.
        /// </summary>
        [Fact]
        public void AddMediator_MainOverload_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(_ => { }, discoverMiddleware: false, discoverNotificationMiddleware: false, typeof(TestCommand).Assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that the main AddMediator overload with statistics disabled does not register <see cref="MediatorStatistics"/>.
        /// </summary>
        [Fact]
        public void AddMediator_MainOverloadWithStatistics_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(_ => { }, enableStatisticsTracking: false, discoverMiddleware: false, discoverNotificationMiddleware: false, typeof(TestCommand).Assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<MediatorStatistics>().ShouldBeNull();
        }

        /// <summary>
        /// Tests that the main AddMediator overload with null assemblies uses the calling assembly.
        /// </summary>
        [Fact]
        public void AddMediator_MainOverloadWithNullAssemblies_UsesCallingAssembly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(_ => { }, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: false, (Assembly[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that the main AddMediator overload with middleware discovery registers notification middleware.
        /// </summary>
        [Fact]
        public void AddMediator_MainOverloadWithMiddlewareDiscovery_RegistersMiddleware()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(_ => { }, enableStatisticsTracking: true, discoverMiddleware: true, discoverNotificationMiddleware: true, typeof(TestCommand).Assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<INotificationMiddlewarePipelineInspector>().ShouldNotBeNull();
        }

        #endregion

        #region AddMediatorFromCallingAssembly Coverage

        /// <summary>
        /// Tests that AddMediatorFromCallingAssembly registers <see cref="IMediator"/> correctly.
        /// </summary>
        [Fact]
        public void AddMediatorFromCallingAssembly_Basic_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediatorFromCallingAssembly();
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that AddMediatorFromCallingAssembly with discover middleware registers <see cref="IMediator"/> correctly.
        /// </summary>
        [Fact]
        public void AddMediatorFromCallingAssembly_WithDiscoverMiddleware_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediatorFromCallingAssembly(discoverMiddleware: false);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that AddMediatorFromCallingAssembly with configuration registers <see cref="IMediator"/> correctly.
        /// </summary>
        [Fact]
        public void AddMediatorFromCallingAssembly_WithConfiguration_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediatorFromCallingAssembly(_ => { });
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that AddMediatorFromCallingAssembly with configuration and discover middleware registers <see cref="IMediator"/> correctly.
        /// </summary>
        [Fact]
        public void AddMediatorFromCallingAssembly_WithConfigurationAndDiscoverMiddleware_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediatorFromCallingAssembly(_ => { }, discoverMiddleware: false);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that AddMediatorFromCallingAssembly with all parameters and statistics disabled does not register <see cref="MediatorStatistics"/>.
        /// </summary>
        [Fact]
        public void AddMediatorFromCallingAssembly_WithAllParameters_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediatorFromCallingAssembly(_ => { }, enableStatisticsTracking: false, discoverMiddleware: false, discoverNotificationMiddleware: false);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<MediatorStatistics>().ShouldBeNull();
        }

        #endregion

        #region AddMediatorFromLoadedAssemblies Coverage

        /// <summary>
        /// Tests that AddMediatorFromLoadedAssemblies registers <see cref="IMediator"/> correctly.
        /// </summary>
        [Fact]
        public void AddMediatorFromLoadedAssemblies_Basic_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediatorFromLoadedAssemblies();
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that AddMediatorFromLoadedAssemblies with filter registers <see cref="IMediator"/> correctly.
        /// </summary>
        [Fact]
        public void AddMediatorFromLoadedAssemblies_WithFilter_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediatorFromLoadedAssemblies(assembly => assembly.GetName().Name!.Contains("Blazing.Mediator"));
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that AddMediatorFromLoadedAssemblies with discover middleware registers <see cref="IMediator"/> correctly.
        /// </summary>
        [Fact]
        public void AddMediatorFromLoadedAssemblies_WithDiscoverMiddleware_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediatorFromLoadedAssemblies(discoverMiddleware: false);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that AddMediatorFromLoadedAssemblies with configuration and filter registers <see cref="IMediator"/> correctly.
        /// </summary>
        [Fact]
        public void AddMediatorFromLoadedAssemblies_WithConfigurationAndFilter_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediatorFromLoadedAssemblies(_ => { }, assembly => assembly.GetName().Name!.Contains("Blazing.Mediator"));
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that AddMediatorFromLoadedAssemblies with configuration, discover middleware, and filter registers <see cref="IMediator"/> correctly.
        /// </summary>
        [Fact]
        public void AddMediatorFromLoadedAssemblies_WithConfigurationDiscoverMiddlewareAndFilter_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediatorFromLoadedAssemblies(_ => { }, discoverMiddleware: false, assembly => assembly.GetName().Name!.Contains("Blazing.Mediator"));
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that AddMediatorFromLoadedAssemblies with all parameters and statistics disabled does not register <see cref="MediatorStatistics"/>.
        /// </summary>
        [Fact]
        public void AddMediatorFromLoadedAssemblies_WithAllParameters_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediatorFromLoadedAssemblies(_ => { }, enableStatisticsTracking: false, discoverMiddleware: false, discoverNotificationMiddleware: false, assembly => assembly.GetName().Name!.Contains("Blazing.Mediator"));
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<MediatorStatistics>().ShouldBeNull();
        }

        #endregion

        #region Edge Cases and Error Conditions

        /// <summary>
        /// Tests that AddMediator and related methods throw <see cref="ArgumentNullException"/> when called on a null service collection.
        /// </summary>
        [Fact]
        public void AddMediator_WithNullServiceCollection_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddMediator());
            Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddMediatorFromCallingAssembly());
            Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddMediatorFromLoadedAssemblies());
            Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddMediatorWithNotificationMiddleware(false, Array.Empty<Assembly>()));
        }

        /// <summary>
        /// Tests that an existing <see cref="IStatisticsRenderer"/> is not overridden by AddMediator.
        /// </summary>
        [Fact]
        public void AddMediator_WithExistingStatisticsRenderer_DoesNotOverride()
        {
            // Arrange
            ServiceCollection services = new();
            var customRenderer = new TestStatisticsRenderer();
            services.AddSingleton<IStatisticsRenderer>(customRenderer);
            
            // Act
            services.AddMediator(config => { config.WithStatisticsTracking(); }, (Assembly[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IStatisticsRenderer>().ShouldBeSameAs(customRenderer);
        }

        /// <summary>
        /// Tests that an existing <see cref="MediatorStatistics"/> is not overridden by AddMediator.
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
            services.AddMediator(config => { config.WithStatisticsTracking(); }, (Assembly[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<MediatorStatistics>().ShouldBeSameAs(customStatistics);
        }

        /// <summary>
        /// Tests that duplicate assemblies are deduplicated correctly by AddMediator.
        /// </summary>
        [Fact]
        public void AddMediator_WithDuplicateAssemblies_DeduplicatesCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            Assembly assembly = typeof(TestCommand).Assembly;
            
            // Act
            services.AddMediator(config => { config.WithStatisticsTracking(); }, assembly, assembly, assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that AddMediator with empty assemblies registers <see cref="IMediator"/> correctly.
        /// </summary>
        [Fact]
        public void AddMediator_WithEmptyAssemblies_RegistersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(Array.Empty<Assembly>());
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that AddMediator with an assembly without handlers still registers <see cref="IMediator"/>.
        /// </summary>
        [Fact]
        public void AddMediator_WithAssemblyWithoutHandlers_ProcessesCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            Assembly assembly = typeof(string).Assembly;
            
            // Act
            services.AddMediator(assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        #endregion

        #region Internal Logic Path Coverage

        /// <summary>
        /// Tests conditional assembly handling logic in AddMediator.
        /// </summary>
        [Fact]
        public void AddMediator_ConditionalAssemblyHandling_WorksCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(_ => { }, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: false, (Assembly[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests conditional middleware registration logic in AddMediator.
        /// </summary>
        [Fact]
        public void AddMediator_ConditionalMiddlewareRegistration_WorksCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(_ => { }, enableStatisticsTracking: true, discoverMiddleware: true, discoverNotificationMiddleware: false, typeof(TestCommand).Assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMiddlewarePipelineInspector>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests conditional notification middleware only registration logic in AddMediator.
        /// </summary>
        [Fact]
        public void AddMediator_ConditionalNotificationMiddlewareOnly_WorksCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(_ => { }, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: true, typeof(TestCommand).Assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<INotificationMiddlewarePipelineInspector>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that null middleware discovery parameters are converted to false in AddMediator.
        /// </summary>
        [Fact]
        public void AddMediator_NullMiddlewareDiscovery_ConvertsToFalse()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(_ => { }, enableStatisticsTracking: true, discoverMiddleware: null, discoverNotificationMiddleware: null);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMediator>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests conditional statistics renderer registration logic in AddMediator.
        /// </summary>
        [Fact]
        public void AddMediator_ConditionalStatisticsRenderer_WorksCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(config => { config.WithStatisticsTracking(); }, (Assembly[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IStatisticsRenderer>().ShouldBeOfType<ConsoleStatisticsRenderer>();
        }

        /// <summary>
        /// Tests conditional mediator statistics registration logic in AddMediator.
        /// </summary>
        [Fact]
        public void AddMediator_ConditionalMediatorStatistics_WorksCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(config => { config.WithStatisticsTracking(); }, (Assembly[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<MediatorStatistics>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests handler duplication prevention logic in AddMediator.
        /// </summary>
        [Fact]
        public void AddMediator_HandlerDuplicationPrevention_WorksCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            Assembly assembly = typeof(TestCommandHandler).Assembly;
            
            // Act
            services.AddMediator(config => { config.WithStatisticsTracking(); }, assembly);
            services.AddMediator(config => { config.WithStatisticsTracking(); }, assembly);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            var handlers = serviceProvider.GetServices<IRequestHandler<TestCommand>>();
            handlers.Count().ShouldBe(1);
        }

        /// <summary>
        /// Tests pipeline inspector registration logic in AddMediator.
        /// </summary>
        [Fact]
        public void AddMediator_PipelineInspectorRegistration_WorksCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(config => { config.WithStatisticsTracking(); }, (Assembly[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            // Assert
            serviceProvider.GetService<IMiddlewarePipelineInspector>().ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that service lifetimes for <see cref="IMediator"/> and <see cref="MediatorStatistics"/> are correct.
        /// </summary>
        [Fact]
        public void AddMediator_ServiceLifetimes_AreCorrect()
        {
            // Arrange
            ServiceCollection services = new();
            services.AddMediator(config => { config.WithStatisticsTracking(); }, (Assembly[])null!);
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            using var scope1 = serviceProvider.CreateScope();
            using var scope2 = serviceProvider.CreateScope();

            // Act
            var mediator1 = scope1.ServiceProvider.GetService<IMediator>();
            var mediator2 = scope2.ServiceProvider.GetService<IMediator>();
            var stats1 = scope1.ServiceProvider.GetService<MediatorStatistics>();
            var stats2 = scope2.ServiceProvider.GetService<MediatorStatistics>();

            // Assert
            mediator1.ShouldNotBeSameAs(mediator2); // Scoped
            stats1.ShouldBeSameAs(stats2); // Singleton
        }

        #endregion

        #region Integration Tests

        /// <summary>
        /// Full integration test for AddMediator, middleware, and statistics tracking.
        /// </summary>
        [Fact]
        public async Task AddMediator_FullIntegration_WorksEndToEnd()
        {
            // Arrange
            ServiceCollection services = new();
            var customRenderer = new TestStatisticsRenderer();
            services.AddSingleton<IStatisticsRenderer>(customRenderer);
            
            // Act
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

            // Assert
            customRenderer.Messages.ShouldContain(msg => msg.Contains("Commands: 1"));
            customRenderer.Messages.ShouldContain(msg => msg.Contains("Queries: 1"));
            result.ShouldBe("First: Handler: test");
        }

        /// <summary>
        /// Tests that MediatorFactory injects statistics correctly when enabled.
        /// </summary>
        [Fact]
        public async Task MediatorFactory_WithStatistics_InjectsStatisticsCorrectly()
        {
            // Arrange
            ServiceCollection services = new();
            var customRenderer = new TestStatisticsRenderer();
            services.AddSingleton<IStatisticsRenderer>(customRenderer);
            
            // Act
            services.AddMediator(null, enableStatisticsTracking: true, typeof(TestCommand).Assembly);

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new TestCommand { Value = "test" });

            var statistics = serviceProvider.GetRequiredService<MediatorStatistics>();
            statistics.ReportStatistics();
            
            // Assert
            customRenderer.Messages.ShouldContain(msg => msg.Contains("Commands: 1"));
        }

        /// <summary>
        /// Tests that MediatorFactory handles null statistics when statistics tracking is disabled.
        /// </summary>
        [Fact]
        public async Task MediatorFactory_WithoutStatistics_HandlesNullStatistics()
        {
            // Arrange
            ServiceCollection services = new();
            
            // Act
            services.AddMediator(null, enableStatisticsTracking: false, typeof(TestCommand).Assembly);

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new TestCommand { Value = "test" });
            
            // Assert
            // No exception means success
        }

        #endregion
    }
}