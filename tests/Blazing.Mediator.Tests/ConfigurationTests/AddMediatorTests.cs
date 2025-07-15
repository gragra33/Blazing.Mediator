using Blazing.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Reflection;
using Xunit;

namespace Blazing.Mediator.Tests.ConfigurationTests;

public class AddMediatorTests
{
    [Fact]
    public void AddMediator_WithoutParameters_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Should.NotThrow(() => services.AddMediator());
    }

    [Fact]
    public void AddMediator_WithoutParameters_ShouldRegisterMediator()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();

        // Assert
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_WithEmptyAssemblyArray_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(Array.Empty<Assembly>());

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();

        // Assert
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_WithNullAssemblyArray_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator((Assembly[])null!);

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();

        // Assert
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_ParameterlessVersion_ShouldBeEquivalentToEmptyAssemblyArray()
    {
        // Arrange
        var services1 = new ServiceCollection();
        var services2 = new ServiceCollection();

        // Act
        services1.AddMediator();
        services2.AddMediator(Array.Empty<Assembly>());

        // Assert
        services1.Count.ShouldBe(services2.Count);
    }
}
