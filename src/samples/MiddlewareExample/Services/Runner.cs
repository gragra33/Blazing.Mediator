using Blazing.Mediator.Statistics;
using Example.Common;

namespace MiddlewareExample.Services;

/// <summary>
/// Runner service that demonstrates an e-commerce scenario using Blazing.Mediator.
/// </summary>
public class Runner(
    IMediator mediator,
    ILogger<Runner> logger,
    ExampleAnalysisService analysisService)
{
    /// <summary>
    /// Inspects and displays the registered middleware pipeline.
    /// </summary>
    public void InspectMiddlewarePipeline()
    {
        analysisService.DisplayRegisteredMiddleware();
    }

    /// <summary>
    /// Orchestrates the complete e-commerce demo workflow.
    /// </summary>
    public async Task Run()
    {
        logger.LogInformation("Starting E-Commerce Demo with Blazing.Mediator...");

        // Display pre-execution analysis
        analysisService.DisplayPreExecutionAnalysis();

        await DemonstrateProductLookup();
        await DemonstrateInventoryManagement();
        await DemonstrateOrderConfirmation();
        await DemonstrateCustomerRegistration();
        await DemonstrateCustomerDetailsUpdate();

        // Display post-execution analysis with detailed statistics
        analysisService.DisplayPostExecutionAnalysis();

        logger.LogInformation("E-Commerce Demo completed!");
    }

    /// <summary>
    /// Demonstrates product query functionality with caching middleware.
    /// </summary>
    private async Task DemonstrateProductLookup()
    {
        logger.LogDebug("-------- PRODUCT LOOKUP --------");

        var productQuery = new GetProductQuery { ProductId = "WIDGET-001" };
        logger.LogDebug(">> Looking up product: {ProductId}", productQuery.ProductId);

        var productInfo = await mediator.Send(productQuery);
        logger.LogDebug("<< Product found: {ProductInfo}", productInfo);
    }

    /// <summary>
    /// Demonstrates inventory management functionality.
    /// </summary>
    private async Task DemonstrateInventoryManagement()
    {
        logger.LogDebug("-------- INVENTORY MANAGEMENT --------");

        var inventoryCommand = new UpdateInventoryCommand
        {
            ProductId = "WIDGET-001",
            QuantityChange = -5  // Simulating a sale of 5 units
        };
        logger.LogDebug(">> Updating inventory for: {ProductId}, change: {QuantityChange}",
            inventoryCommand.ProductId, inventoryCommand.QuantityChange);

        var newStockCount = await mediator.Send(inventoryCommand);
        logger.LogDebug("<< New stock count: {StockCount} units", newStockCount);
    }

    /// <summary>
    /// Demonstrates order confirmation email functionality.
    /// </summary>
    private async Task DemonstrateOrderConfirmation()
    {
        logger.LogDebug("-------- ORDER CONFIRMATION --------");

        var emailCommand = new SendOrderConfirmationCommand
        {
            OrderId = "ORD-2025-001",
            CustomerEmail = "customer@example.com"
        };
        logger.LogDebug(">> Sending order confirmation for: {OrderId} to: {CustomerEmail}",
            emailCommand.OrderId, emailCommand.CustomerEmail);

        await mediator.Send(emailCommand);
        logger.LogDebug("<< Order confirmation sent successfully!");
    }

    /// <summary>
    /// Demonstrates customer registration with validation error handling.
    /// </summary>
    private async Task DemonstrateCustomerRegistration()
    {
        logger.LogDebug("-------- CUSTOMER REGISTRATION (With Validation) --------");

        try
        {
            var registerCustomer = new RegisterCustomerCommand
            {
                FullName = "J",  // This will trigger validation error (too short)
                Email = "john.doe@example.com",
                ContactMethod = "Email"
            };

            logger.LogDebug(">> Registering customer: {FullName} ({Email})",
                registerCustomer.FullName, registerCustomer.Email);

            await mediator.Send(registerCustomer);
            logger.LogDebug("<< Customer registered successfully!");
        }
        catch (InvalidOperationException ex) when (ex.InnerException is ValidationException)
        {
            logger.LogError("!! Customer registration failed due to validation errors: {ErrorMessage}", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "!! Unexpected error during customer registration");
        }
    }

    /// <summary>
    /// Demonstrates customer details update with validation error handling and response handling.
    /// Uses middleware implementations that handle both request and response types (&lt;TRequest, TResponse&gt;),
    /// allowing for more sophisticated processing of commands that return values.
    /// </summary>
    private async Task DemonstrateCustomerDetailsUpdate()
    {
        logger.LogDebug("-------- CUSTOMER DETAILS UPDATE (With Validation Error & Retry Success) --------");

        // First attempt with invalid data to show validation in action
        try
        {
            var updateCustomer = new UpdateCustomerDetailsCommand
            {
                CustomerId = "INVALID-ID",  // This will trigger validation error (wrong format)
                FullName = "John Doe",
                Email = "john.doe@example.com",
                ContactMethod = "Email"
            };

            logger.LogDebug(">> Updating customer details (invalid data): {CustomerId} - {FullName} ({Email})",
                updateCustomer.CustomerId, updateCustomer.FullName, updateCustomer.Email);

            var isSuccess = await mediator.Send(updateCustomer);

            if (isSuccess)
            {
                logger.LogDebug("<< Customer details updated successfully!");
            }
            else
            {
                logger.LogWarning("<< Customer details update failed!");
            }
        }
        catch (InvalidOperationException ex) when (ex.InnerException is ValidationException)
        {
            logger.LogError("!! Customer details update failed due to validation errors: {ErrorMessage}", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "!! Unexpected error during customer details update");
        }

        // Second attempt with valid data to show successful execution
        try
        {
            var updateCustomerValid = new UpdateCustomerDetailsCommand
            {
                CustomerId = "CUST-123456",  // Valid format
                FullName = "John Doe",
                Email = "john.doe@example.com",
                ContactMethod = "Email"
            };

            logger.LogDebug(">> Updating customer details (valid data): {CustomerId} - {FullName} ({Email})",
                updateCustomerValid.CustomerId, updateCustomerValid.FullName, updateCustomerValid.Email);

            var isSuccess = await mediator.Send(updateCustomerValid);

            if (isSuccess)
            {
                logger.LogDebug("<< Customer details updated successfully!");
            }
            else
            {
                logger.LogWarning("<< Customer details update failed!");
            }
        }
        catch (InvalidOperationException ex) when (ex.InnerException is ValidationException)
        {
            logger.LogError("!! Customer details update failed due to validation errors: {ErrorMessage}", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "!! Unexpected error during customer details update");
        }
    }
}
