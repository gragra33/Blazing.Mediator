using Microsoft.Extensions.DependencyInjection;
using Blazing.Mediator.Statistics;

namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Tests for custom domain interface detection in mediator statistics analysis.
/// Verifies that custom domain interfaces like ICustomerRequest, IProductRequest are correctly
/// prioritized over built-in interfaces like IRequest, ICommand in analysis results.
/// </summary>
public class CustomDomainInterfaceAnalysisTests : IDisposable
{
    private readonly ServiceCollection _services;
    private readonly ServiceProvider _serviceProvider;
    private readonly MediatorStatistics _mediatorStatistics;

    public CustomDomainInterfaceAnalysisTests()
    {
        _services = new ServiceCollection();
        _services.AddMediator(typeof(CustomDomainInterfaceAnalysisTests).Assembly);
        _services.AddLogging();
        
        _serviceProvider = _services.BuildServiceProvider();
        _mediatorStatistics = new MediatorStatistics(new CustomTestStatisticsRenderer());
    }

    [Fact]
    public void AnalyzeQueries_ShouldPrioritizeCustomDomainInterface_OverBuiltInInterface()
    {
        // Act
        var results = _mediatorStatistics.AnalyzeQueries(_serviceProvider, isDetailed: true);

        // Assert
        var customerQuery = results.FirstOrDefault(r => r.ClassName == "GetCustomerQuery");
        Assert.NotNull(customerQuery);
        Assert.Equal("ICustomerRequest<CustomerDto>", customerQuery.PrimaryInterface);
        Assert.NotEqual("IRequest<CustomerDto>", customerQuery.PrimaryInterface);
    }

    [Fact]
    public void AnalyzeQueries_ShouldPrioritizeProductInterface_OverBuiltInInterface()
    {
        // Act
        var results = _mediatorStatistics.AnalyzeQueries(_serviceProvider, isDetailed: true);

        // Assert
        var productQuery = results.FirstOrDefault(r => r.ClassName == "GetProductQuery");
        Assert.NotNull(productQuery);
        Assert.Equal("IProductRequest<ProductDto>", productQuery.PrimaryInterface);
        Assert.NotEqual("IRequest<ProductDto>", productQuery.PrimaryInterface);
    }

    [Fact]
    public void AnalyzeCommands_ShouldPrioritizeCustomerCommandInterface_OverBuiltInInterface()
    {
        // Act
        var results = _mediatorStatistics.AnalyzeCommands(_serviceProvider, isDetailed: true);

        // Assert
        var customerCommand = results.FirstOrDefault(r => r.ClassName == "UpdateCustomerCommand");
        Assert.NotNull(customerCommand);
        Assert.Equal("ICustomerRequest<Boolean>", customerCommand.PrimaryInterface);
        Assert.NotEqual("IRequest<Boolean>", customerCommand.PrimaryInterface);
    }

    [Fact]
    public void AnalyzeCommands_ShouldPrioritizeVoidCustomerCommand_OverBuiltInInterface()
    {
        // Act
        var results = _mediatorStatistics.AnalyzeCommands(_serviceProvider, isDetailed: true);

        // Assert
        var voidCustomerCommand = results.FirstOrDefault(r => r.ClassName == "CreateCustomerCommand");
        Assert.NotNull(voidCustomerCommand);
        Assert.Equal("ICustomerRequest", voidCustomerCommand.PrimaryInterface);
        Assert.NotEqual("IRequest", voidCustomerCommand.PrimaryInterface);
    }

    [Fact]
    public void AnalyzeCommands_ShouldFallBackToBuiltInInterface_WhenNoCustomInterfaceExists()
    {
        // Act
        var results = _mediatorStatistics.AnalyzeCommands(_serviceProvider, isDetailed: true);

        // Assert
        var builtInCommand = results.FirstOrDefault(r => r.ClassName == "StandardRequestCommand");
        Assert.NotNull(builtInCommand);
        Assert.Equal("IRequest<String>", builtInCommand.PrimaryInterface);
    }

    [Fact]
    public void AnalyzeQueries_ShouldHandleMostSpecificInterface_WhenMultipleCustomInterfacesExist()
    {
        // Act
        var results = _mediatorStatistics.AnalyzeQueries(_serviceProvider, isDetailed: true);

        // Assert
        var multiInterfaceQuery = results.FirstOrDefault(r => r.ClassName == "GetPremiumCustomerQuery");
        Assert.NotNull(multiInterfaceQuery);
        // Should prioritize the most specific interface (IPremiumCustomerRequest over ICustomerRequest)
        Assert.Equal("IPremiumCustomerRequest<PremiumCustomerDto>", multiInterfaceQuery.PrimaryInterface);
    }

    [Fact]
    public void InterfaceSpecificityCalculation_ShouldPrioritizeDomainSpecificInterfaces()
    {
        // Act
        var results = _mediatorStatistics.AnalyzeQueries(_serviceProvider, isDetailed: true);

        // Assert
        var inventoryQuery = results.FirstOrDefault(r => r.ClassName == "GetInventoryQuery");
        Assert.NotNull(inventoryQuery);
        Assert.Equal("IInventoryRequest<InventoryDto>", inventoryQuery.PrimaryInterface);
    }

    [Fact]
    public void AnalyzeQueries_CompactMode_ShouldStillShowCustomInterfaces()
    {
        // Act
        var results = _mediatorStatistics.AnalyzeQueries(_serviceProvider, isDetailed: false);

        // Assert
        var customerQuery = results.FirstOrDefault(r => r.ClassName == "GetCustomerQuery");
        Assert.NotNull(customerQuery);
        Assert.Equal("ICustomerRequest<CustomerDto>", customerQuery.PrimaryInterface);
        
        // In compact mode, type parameters should be empty
        Assert.Empty(customerQuery.TypeParameters);
    }

    [Fact]
    public void AnalyzeCommands_ShouldCorrectlyDetectResponseTypes_FromCustomInterfaces()
    {
        // Act
        var results = _mediatorStatistics.AnalyzeCommands(_serviceProvider, isDetailed: true);

        // Assert
        var customerCommand = results.FirstOrDefault(r => r.ClassName == "UpdateCustomerCommand");
        Assert.NotNull(customerCommand);
        Assert.Equal(typeof(bool), customerCommand.ResponseType);
        Assert.Equal("ICustomerRequest<Boolean>", customerCommand.PrimaryInterface);
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
        _mediatorStatistics?.Dispose();
    }
}

#region Test Domain Interfaces

/// <summary>
/// Custom customer domain interface for testing.
/// </summary>
public interface ICustomerRequest : IRequest
{
}

/// <summary>
/// Custom customer domain interface with response for testing.
/// </summary>
public interface ICustomerRequest<out TResponse> : IRequest<TResponse>
{
}

/// <summary>
/// Custom product domain interface for testing.
/// </summary>
public interface IProductRequest<out TResponse> : IRequest<TResponse>
{
}

/// <summary>
/// Custom inventory domain interface for testing.
/// </summary>
public interface IInventoryRequest<out TResponse> : IRequest<TResponse>
{
}

/// <summary>
/// More specific customer interface for testing interface priority.
/// </summary>
public interface IPremiumCustomerRequest<out TResponse> : ICustomerRequest<TResponse>
{
}

#endregion

#region Test DTOs

public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class InventoryDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class PremiumCustomerDto : CustomerDto
{
    public string PremiumLevel { get; set; } = string.Empty;
}

#endregion

#region Test Requests

/// <summary>
/// Query that implements custom customer interface - should prioritize ICustomerRequest over IRequest.
/// </summary>
public class GetCustomerQuery : ICustomerRequest<CustomerDto>
{
    public int CustomerId { get; set; }
}

/// <summary>
/// Query that implements custom product interface - should prioritize IProductRequest over IRequest.
/// </summary>
public class GetProductQuery : IProductRequest<ProductDto>
{
    public int ProductId { get; set; }
}

/// <summary>
/// Query that implements custom inventory interface.
/// </summary>
public class GetInventoryQuery : IInventoryRequest<InventoryDto>
{
    public int ProductId { get; set; }
}

/// <summary>
/// Query that implements multiple custom interfaces - should prioritize the most specific one.
/// </summary>
public class GetPremiumCustomerQuery : IPremiumCustomerRequest<PremiumCustomerDto>
{
    public int CustomerId { get; set; }
}

/// <summary>
/// Command with response that implements custom customer interface.
/// </summary>
public class UpdateCustomerCommand : ICustomerRequest<bool>
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Void command that implements custom customer interface.
/// </summary>
public class CreateCustomerCommand : ICustomerRequest
{
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Standard command with only built-in interfaces for fallback testing.
/// </summary>
public class StandardRequestCommand : IRequest<string>
{
    public string Data { get; set; } = string.Empty;
}

#endregion

#region Test Handlers

public class GetCustomerQueryHandler : IRequestHandler<GetCustomerQuery, CustomerDto>
{
    public Task<CustomerDto> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new CustomerDto { Id = request.CustomerId, Name = "Test Customer" });
    }
}

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductDto>
{
    public Task<ProductDto> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ProductDto { Id = request.ProductId, Name = "Test Product" });
    }
}

public class GetInventoryQueryHandler : IRequestHandler<GetInventoryQuery, InventoryDto>
{
    public Task<InventoryDto> Handle(GetInventoryQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new InventoryDto { ProductId = request.ProductId, Quantity = 100 });
    }
}

public class GetPremiumCustomerQueryHandler : IRequestHandler<GetPremiumCustomerQuery, PremiumCustomerDto>
{
    public Task<PremiumCustomerDto> Handle(GetPremiumCustomerQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new PremiumCustomerDto 
        { 
            Id = request.CustomerId, 
            Name = "Premium Customer", 
            PremiumLevel = "Gold" 
        });
    }
}

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, bool>
{
    public Task<bool> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand>
{
    public Task Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public class StandardRequestCommandHandler : IRequestHandler<StandardRequestCommand, string>
{
    public Task<string> Handle(StandardRequestCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult("Processed");
    }
}

#endregion

/// <summary>
/// Custom test renderer for statistics output to avoid conflicts.
/// </summary>
public class CustomTestStatisticsRenderer : IStatisticsRenderer
{
    public void Render(string message)
    {
        // No-op for tests
    }
}