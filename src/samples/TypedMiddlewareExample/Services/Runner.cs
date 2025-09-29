using TypedMiddlewareExample.Commands;
using TypedMiddlewareExample.Queries;

namespace TypedMiddlewareExample.Services;

/// <summary>
/// Service responsible for running the TypedMiddlewareExample demonstration.
/// Shows the distinction between ICommand and IQuery processing with validation middleware.
/// </summary>
public class Runner(
    IMediator mediator,
    ILogger<Runner> logger,
    ExampleAnalysisService analysisService)
{
    /// <summary>
    /// Inspects and displays the middleware pipeline configuration.
    /// </summary>
    public void InspectMiddlewarePipeline()
    {
        analysisService.DisplayRegisteredMiddleware();
    }

    /// <summary>
    /// Runs the complete demonstration showing ICommand vs IQuery processing.
    /// </summary>
    public async Task Run()
    {
        logger.LogInformation("Starting TypedMiddlewareExample Demo with Blazing.Mediator...");
        Console.WriteLine();

        // Display pre-execution analysis
        analysisService.DisplayPreExecutionAnalysis();

        // Demonstrate each type of operation
        await DemonstrateProductLookup();
        await DemonstrateInventoryManagement();
        await DemonstrateOrderConfirmation();
        await DemonstrateCustomerRegistration();
        await DemonstrateCustomerDetailsUpdate();

        // Display post-execution analysis with detailed statistics
        analysisService.DisplayPostExecutionAnalysis();

        logger.LogInformation("TypedMiddlewareExample Demo completed!");
    }

    /// <summary>
    /// Demonstrates product lookup using IQuery (no validation).
    /// </summary>
    private async Task DemonstrateProductLookup()
    {
        Console.WriteLine("-------- PRODUCT LOOKUP (IProductRequest) --------");

        var productQuery = new GetProductQuery { ProductId = "WIDGET-001" };
        logger.LogDebug(">> Looking up product: {ProductId}", productQuery.ProductId);

        var productInfo = await mediator.Send(productQuery);
        logger.LogDebug("<< Product found: {ProductInfo}", productInfo);

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates inventory management using ICommand&lt;T&gt; (with validation).
    /// </summary>
    private async Task DemonstrateInventoryManagement()
    {
        Console.WriteLine("-------- INVENTORY MANAGEMENT (IInventoryRequest) --------");
        logger.LogDebug(">> Updating inventory for: WIDGET-001, change: -5");

        var command = new UpdateInventoryCommand { ProductId = "WIDGET-001", InventoryChange = -5 };
        var newCount = await mediator.Send(command);

        logger.LogDebug("<< New stock count: {NewCount} units", newCount);
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates order confirmation using ICommand (with validation).
    /// </summary>
    private async Task DemonstrateOrderConfirmation()
    {
        Console.WriteLine("-------- ORDER CONFIRMATION (IOrderRequest) --------");
        logger.LogDebug(">> Sending order confirmation for: ORD-2025-001 to: customer@example.com");

        var command = new SendOrderConfirmationCommand
        {
            OrderId = "ORD-2025-001",
            CustomerEmail = "customer@example.com"
        };
        await mediator.Send(command);

        logger.LogDebug("<< Order confirmation sent successfully!");
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates customer registration with validation failure and success.
    /// </summary>
    private async Task DemonstrateCustomerRegistration()
    {
        Console.WriteLine("-------- CUSTOMER REGISTRATION (ICustomerRequest with Validation) --------");

        // First attempt with invalid data (validation should fail)
        logger.LogDebug(">> Registering customer: J (john.doe@example.com)");

        try
        {
            var invalidCommand = new RegisterCustomerCommand
            {
                FullName = "J",  // Too short - will fail validation
                Email = "john.doe@example.com",
                ContactMethod = "Email"
            };
            await mediator.Send(invalidCommand);
        }
        catch (FluentValidation.ValidationException ex)
        {
            // Expected - validation middleware caught this
            logger.LogInformation("-- Expected validation failure caught: {ErrorMessage}", ex.Message);
        }

        Console.WriteLine();

        // Second attempt with valid data
        logger.LogDebug(">> Registering customer: John Doe (john.doe@example.com)");

        var validCommand = new RegisterCustomerCommand
        {
            FullName = "John Doe",  // Valid name
            Email = "john.doe@example.com",
            ContactMethod = "Email"
        };
        await mediator.Send(validCommand);

        logger.LogDebug("<< Customer registered successfully!");
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates customer details update with validation failure and retry success.
    /// </summary>
    private async Task DemonstrateCustomerDetailsUpdate()
    {
        Console.WriteLine("-------- CUSTOMER DETAILS UPDATE (ICustomerRequest<T> with Validation Error & Retry Success) --------");

        // First attempt with invalid data
        logger.LogDebug(">> Updating customer details (invalid data): INVALID-ID - John Doe (john.doe@example.com)");

        try
        {
            var invalidCommand = new UpdateCustomerDetailsCommand
            {
                CustomerId = "INVALID-ID",  // Invalid format - will fail validation
                FullName = "John Doe",
                Email = "john.doe@example.com",
                ContactMethod = "Email"
            };
            await mediator.Send(invalidCommand);
        }
        catch (FluentValidation.ValidationException ex)
        {
            // Expected - validation middleware caught this
            logger.LogInformation("-- Expected validation failure caught: {ErrorMessage}", ex.Message);
        }

        Console.WriteLine();

        // Second attempt with valid data
        logger.LogDebug(">> Updating customer details (valid data): CUST-123456 - John Doe (john.doe@example.com)");

        var validCommand = new UpdateCustomerDetailsCommand
        {
            CustomerId = "CUST-123456",  // Valid format
            FullName = "John Doe",
            Email = "john.doe@example.com",
            ContactMethod = "Email"
        };
        await mediator.Send(validCommand);

        logger.LogDebug("<< Customer details updated successfully!");
        Console.WriteLine();
    }
}