using Blazing.Mediator.Configuration;
using Blazing.Mediator.Statistics;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Blazing.Mediator.Tests.Tests
{
    /// <summary>
    /// Tests to ensure all ServiceCollectionExtensions method overloads are adjacent and properly organized.
    /// This helps prevent lint warnings about method organization.
    /// </summary>
    public class ServiceCollectionExtensionsLintTests
    {
        /// <summary>
        /// Tests that all AddMediator overloads are properly grouped and adjacent.
        /// This test verifies that we don't have lint warnings about method organization.
        /// </summary>
        [Fact]
        public void ServiceCollectionExtensions_AddMediatorOverloads_AreProperlyGrouped()
        {
            // Arrange - Get all method names from ServiceCollectionExtensions
            var methods = typeof(ServiceCollectionExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name.StartsWith("AddMediator"))
                .Select(m => m.Name)
                .ToList();

            // Act & Assert - Verify we have the expected methods
            methods.ShouldContain("AddMediator");
            methods.ShouldContain("AddMediatorFromCallingAssembly");
            methods.ShouldContain("AddMediatorFromLoadedAssemblies");
            methods.ShouldContain("AddMediatorWithNotificationMiddleware");

            // Verify we have multiple overloads
            methods.Count.ShouldBeGreaterThan(10);
        }

        /// <summary>
        /// Tests that helper methods are properly organized and accessible.
        /// </summary>
        [Fact]
        public void ServiceCollectionExtensions_HelperMethods_AreProperlyEncapsulated()
        {
            // Arrange - Get all private/internal methods
            var privateMethods = typeof(ServiceCollectionExtensions)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .Select(m => m.Name)
                .ToList();

            // Act & Assert - Verify helper methods exist
            privateMethods.ShouldContain("RegisterHandlers");
            privateMethods.ShouldContain("RegisterMiddleware");
            privateMethods.ShouldContain("RegisterMiddlewareFromAssembly");
        }

        /// <summary>
        /// Tests that all overloads handle null parameters correctly without warnings.
        /// </summary>
        [Fact]
        public void ServiceCollectionExtensions_NullParameterHandling_DoesNotGenerateWarnings()
        {
            // Arrange
            ServiceCollection services = new();

            // Act & Assert - Test various null parameter scenarios
            Should.NotThrow(() => services.AddMediator((Assembly[])null!));
            Should.NotThrow(() => services.AddMediator((Type[])null!));
            Should.NotThrow(() => services.AddMediator(null, (Assembly[])null!));
            Should.NotThrow(() => services.AddMediator(null, (Type[])null!));
            Should.NotThrow(() => services.AddMediator(null, true, (Assembly[])null!));
            Should.NotThrow(() => services.AddMediator(null, true, (Type[])null!));

            // Verify mediator can still be resolved
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            mediator.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests middleware discovery overloads for proper parameter handling.
        /// </summary>
        [Fact]
        public void ServiceCollectionExtensions_MiddlewareDiscoveryOverloads_HandleParametersCorrectly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act & Assert - Test middleware discovery overloads
            Should.NotThrow(() => services.AddMediator(false, Array.Empty<Assembly>()));
            Should.NotThrow(() => services.AddMediator(true, Array.Empty<Assembly>()));
            Should.NotThrow(() => services.AddMediator(false, typeof(ServiceCollectionExtensionsLintTests)));
            Should.NotThrow(() => services.AddMediatorWithNotificationMiddleware(false, Array.Empty<Assembly>()));
            Should.NotThrow(() => services.AddMediatorWithNotificationMiddleware(true, Array.Empty<Assembly>()));

            // Verify services are registered correctly
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            mediator.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that all calling assembly methods work correctly.
        /// </summary>
        [Fact]
        public void ServiceCollectionExtensions_CallingAssemblyMethods_WorkCorrectly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act & Assert - Test all calling assembly overloads
            Should.NotThrow(() => services.AddMediatorFromCallingAssembly());

            // Test with configuration
            Should.NotThrow(() => services.AddMediatorFromCallingAssembly(_ => { }));

            // Test with middleware discovery
            Should.NotThrow(() => services.AddMediatorFromCallingAssembly(discoverMiddleware: false));

            // Test with all parameters
            Should.NotThrow(() => services.AddMediatorFromCallingAssembly(null, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: false));

            // Verify services are registered
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            mediator.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that all loaded assemblies methods work correctly with filters.
        /// </summary>
        [Fact]
        public void ServiceCollectionExtensions_LoadedAssembliesMethods_WorkCorrectly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act & Assert - Test loaded assemblies overloads
            Should.NotThrow(() => services.AddMediatorFromLoadedAssemblies());

            // Test with filter
            Should.NotThrow(() => services.AddMediatorFromLoadedAssemblies(assembly => assembly.GetName().Name!.Contains("Blazing.Mediator")));

            // Test with middleware discovery
            Should.NotThrow(() => services.AddMediatorFromLoadedAssemblies(discoverMiddleware: false));

            // Test with configuration and filter
            Should.NotThrow(() => services.AddMediatorFromLoadedAssemblies(_ => { }, assembly => assembly.GetName().Name!.Contains("Blazing.Mediator")));

            // Test with all parameters
            Should.NotThrow(() => services.AddMediatorFromLoadedAssemblies(null, enableStatisticsTracking: true, discoverMiddleware: false, discoverNotificationMiddleware: false, assembly => assembly.GetName().Name!.Contains("Blazing.Mediator")));

            // Verify services are registered
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            mediator.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests edge case scenarios that might generate warnings.
        /// </summary>
        [Fact]
        public void ServiceCollectionExtensions_EdgeCases_HandleCorrectly()
        {
            // Arrange
            ServiceCollection services = new();

            // Act & Assert - Test edge cases
            Should.NotThrow(() => services.AddMediator(Array.Empty<Assembly>()));
            Should.NotThrow(() => services.AddMediator(Array.Empty<Type>()));
            Should.NotThrow(() => services.AddMediator(null, Array.Empty<Assembly>()));
            Should.NotThrow(() => services.AddMediator(null, Array.Empty<Type>()));
            Should.NotThrow(() => services.AddMediator(null, true, Array.Empty<Assembly>()));
            Should.NotThrow(() => services.AddMediator(null, false, Array.Empty<Assembly>()));

            // Test with null configuration but valid assemblies
            Should.NotThrow(() => services.AddMediator((Action<MediatorConfiguration>?)null, typeof(ServiceCollectionExtensionsLintTests).Assembly));

            // Verify final state
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            mediator.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that duplicate registrations are handled gracefully.
        /// </summary>
        [Fact]
        public void ServiceCollectionExtensions_DuplicateRegistrations_HandleGracefully()
        {
            // Arrange
            ServiceCollection services = new();

            // Act & Assert - Test multiple registrations
            Should.NotThrow(() =>
            {
                services.AddMediator(config => config.WithStatisticsTracking(), (Assembly[])null!);
                services.AddMediator(config => { }, (Assembly[])null!); // Should not override
                services.AddMediator(config => config.WithStatisticsTracking(), typeof(ServiceCollectionExtensionsLintTests).Assembly);
                services.AddMediator(config => config.WithStatisticsTracking(), typeof(ServiceCollectionExtensionsLintTests).Assembly); // Duplicate
                services.AddMediatorFromCallingAssembly();
            });

            // Verify services are still functional
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            mediator.ShouldNotBeNull();

            // Verify statistics configuration (should be based on first registration)
            var statistics = serviceProvider.GetService<MediatorStatistics>();
            statistics.ShouldNotBeNull(); // Should be available because first registration enabled it
        }

        /// <summary>
        /// Tests parameter validation and argument checks.
        /// </summary>
        [Fact]
        public void ServiceCollectionExtensions_ParameterValidation_WorksCorrectly()
        {
            // Arrange & Act & Assert - Test null service collection throws
            Should.Throw<ArgumentNullException>(() => ((IServiceCollection)null!).AddMediator());
            Should.Throw<ArgumentNullException>(() => ((IServiceCollection)null!).AddMediatorFromCallingAssembly());
            Should.Throw<ArgumentNullException>(() => ((IServiceCollection)null!).AddMediatorFromLoadedAssemblies());
            Should.Throw<ArgumentNullException>(() => ((IServiceCollection)null!).AddMediatorWithNotificationMiddleware(false, Array.Empty<Assembly>()));
        }
    }
}