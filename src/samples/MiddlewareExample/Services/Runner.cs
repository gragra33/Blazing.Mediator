using Blazing.Mediator.Statistics;
using Blazing.Mediator.Abstractions;

namespace MiddlewareExample.Services;

/// <summary>
/// Runner service that demonstrates an e-commerce scenario using Blazing.Mediator.
/// </summary>
public class Runner(
    IMediator mediator,
    ILogger<Runner> logger,
    IMiddlewarePipelineInspector pipelineInspector,
    IServiceProvider serviceProvider,
    MediatorStatistics mediatorStatistics)
{
    /// <summary>
    /// Inspects and displays the registered middleware pipeline.
    /// </summary>
    public void InspectMiddlewarePipeline()
    {
        // Use the built-in analysis method from the core library
        var middlewareAnalysis = pipelineInspector.AnalyzeMiddleware(serviceProvider);

        Console.WriteLine("Registered middleware:");
        foreach (var middleware in middlewareAnalysis)
        {
            Console.WriteLine($"  - [{middleware.OrderDisplay}] {middleware.ClassName}{middleware.TypeParameters}");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates the new mediator statistics functionality for analyzing queries and commands.
    /// </summary>
    public void InspectQueriesAndCommands()
    {
        // === MEDIATOR ANALYSIS ===
        logger.LogInformation("=== MEDIATOR ANALYSIS ===");
        
        // Analyze all queries in the application
        var queries = mediatorStatistics.AnalyzeQueries(serviceProvider);
        logger.LogInformation("");
        logger.LogInformation("* QUERIES DISCOVERED:");
        
        if (queries.Any())
        {
            var queryGroups = queries.GroupBy(q => q.Assembly);
            foreach (var assemblyGroup in queryGroups)
            {
                logger.LogInformation("  * Assembly: {Assembly}", assemblyGroup.Key);
                var namespaceGroups = assemblyGroup.GroupBy(q => q.Namespace);
                foreach (var namespaceGroup in namespaceGroups)
                {
                    logger.LogInformation("    * {Namespace}", namespaceGroup.Key);
                    foreach (var query in namespaceGroup)
                    {
                        var statusIcon = query.HandlerStatus switch
                        {
                            HandlerStatus.Single => "+",
                            HandlerStatus.Missing => "!",
                            HandlerStatus.Multiple => "#",
                            _ => "?"
                        };
                        var responseType = query.ResponseType?.Name ?? "void";
                        logger.LogInformation("      {StatusIcon} {ClassName}{TypeParameters} -> {ResponseType} ({HandlerDetails})", 
                            statusIcon, query.ClassName, query.TypeParameters, responseType, query.HandlerDetails);
                    }
                }
            }
        }
        else
        {
            logger.LogInformation("  (No queries discovered)");
        }

        // Analyze all commands in the application
        var commands = mediatorStatistics.AnalyzeCommands(serviceProvider);
        logger.LogInformation("");
        logger.LogInformation("* COMMANDS DISCOVERED:");
        
        if (commands.Any())
        {
            var commandGroups = commands.GroupBy(c => c.Assembly);
            foreach (var assemblyGroup in commandGroups)
            {
                logger.LogInformation("  * Assembly: {Assembly}", assemblyGroup.Key);
                var namespaceGroups = assemblyGroup.GroupBy(c => c.Namespace);
                foreach (var namespaceGroup in namespaceGroups)
                {
                    logger.LogInformation("    * {Namespace}", namespaceGroup.Key);
                    foreach (var command in namespaceGroup)
                    {
                        var statusIcon = command.HandlerStatus switch
                        {
                            HandlerStatus.Single => "+",
                            HandlerStatus.Missing => "!",
                            HandlerStatus.Multiple => "#",
                            _ => "?"
                        };
                        var responseType = command.ResponseType?.Name ?? "void";
                        logger.LogInformation("      {StatusIcon} {ClassName}{TypeParameters} -> {ResponseType} ({HandlerDetails})", 
                            statusIcon, command.ClassName, command.TypeParameters, responseType, command.HandlerDetails);
                    }
                }
            }
        }
        else
        {
            logger.LogInformation("  (No commands discovered)");
        }
        
        logger.LogInformation("");
        logger.LogInformation("Legend: + = Handler found, ! = No handler, # = Multiple handlers");
        logger.LogInformation("=========================");
        logger.LogInformation("");
    }

    /// <summary>
    /// Orchestrates the complete e-commerce demo workflow.
    /// </summary>
    public async Task Run()
    {
        logger.LogInformation("Starting E-Commerce Demo with Blazing.Mediator...");

        // First, analyze the application structure
        InspectQueriesAndCommands();

        await DemonstrateProductLookup();
        await DemonstrateInventoryManagement();
        await DemonstrateOrderConfirmation();
        await DemonstrateCustomerRegistration();
        await DemonstrateCustomerDetailsUpdate();

        // Show final statistics
        Console.WriteLine("=== EXECUTION STATISTICS ===");
        mediatorStatistics.ReportStatistics();
        Console.WriteLine("=============================");
        Console.WriteLine();

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
