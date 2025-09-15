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
    private void InspectMediatorTypes()
    {
        logger.LogInformation("=== COMPREHENSIVE MEDIATOR ANALYSIS ===");
        logger.LogInformation("");

        // Show compact analysis first
        logger.LogInformation("COMPACT MODE (isDetailed: false):");
        logger.LogInformation("══════════════════════════════════════");
        ShowCompactAnalysis();
        
        logger.LogInformation("");
        logger.LogInformation("DETAILED MODE (isDetailed: true - Default):");
        logger.LogInformation("════════════════════════════════════════════════");
        ShowDetailedAnalysis();
    }

    /// <summary>
    /// Shows compact analysis output (isDetailed: false).
    /// </summary>
    private void ShowCompactAnalysis()
    {
        // Analyze all queries in compact mode
        var queries = mediatorStatistics.AnalyzeQueries(serviceProvider, isDetailed: false);
        logger.LogInformation($"* {queries.Count} QUERIES DISCOVERED:");
        if (queries.Any())
        {
            var queryGroups = queries.GroupBy(q => q.Assembly);
            foreach (var assemblyGroup in queryGroups)
            {
                logger.LogInformation("  * Assembly: {Assembly}", assemblyGroup.Key);
                var namespaceGroups = assemblyGroup.GroupBy(q => q.Namespace);
                foreach (var namespaceGroup in namespaceGroups)
                {
                    logger.LogInformation("    * Namespace: {Namespace}", namespaceGroup.Key);
                    foreach (var query in namespaceGroup)
                    {
                        var statusIcon = query.HandlerStatus switch
                        {
                            HandlerStatus.Single => "+",
                            HandlerStatus.Missing => "!",
                            HandlerStatus.Multiple => "#",
                            _ => "?"
                        };
                        
                        logger.LogInformation("      {StatusIcon} {ClassName}{TypeParameters} : {PrimaryInterface}", statusIcon, query.ClassName, query.TypeParameters, query.PrimaryInterface);
                    }
                }
            }
        }
        else
        {
            logger.LogInformation("  (No queries discovered)");
        }

        var commands = mediatorStatistics.AnalyzeCommands(serviceProvider, isDetailed: false);
        logger.LogInformation("");
        logger.LogInformation($"* {commands.Count} COMMANDS DISCOVERED:");
        if (commands.Any())
        {
            var commandGroups = commands.GroupBy(c => c.Assembly);
            foreach (var assemblyGroup in commandGroups)
            {
                logger.LogInformation("  * Assembly: {Assembly}", assemblyGroup.Key);
                var namespaceGroups = assemblyGroup.GroupBy(c => c.Namespace);
                foreach (var namespaceGroup in namespaceGroups)
                {
                    logger.LogInformation("    * Namespace: {Namespace}", namespaceGroup.Key);
                    foreach (var command in namespaceGroup)
                    {
                        var statusIcon = command.HandlerStatus switch
                        {
                            HandlerStatus.Single => "+",
                            HandlerStatus.Missing => "!",
                            HandlerStatus.Multiple => "#",
                            _ => "?"
                        };
                        
                        logger.LogInformation("      {StatusIcon} {ClassName}{TypeParameters} : {PrimaryInterface}", statusIcon, command.ClassName, command.TypeParameters, command.PrimaryInterface);
                    }
                }
            }
        }
        else
        {
            logger.LogInformation("  (No commands discovered)");
        }
        
        logger.LogInformation("");
        logger.LogInformation("LEGEND: + = Handler found, ! = No handler, # = Multiple handlers");
    }

    /// <summary>
    /// Shows detailed analysis output (isDetailed: true - default).
    /// </summary>
    private void ShowDetailedAnalysis()
    {
        // Analyze all queries in detailed mode (default)
        var queries = mediatorStatistics.AnalyzeQueries(serviceProvider);
        logger.LogInformation($"* {queries.Count} QUERIES DISCOVERED:");
        if (queries.Any())
        {
            var queryGroups = queries.GroupBy(q => q.Assembly);
            foreach (var assemblyGroup in queryGroups)
            {
                logger.LogInformation("  * Assembly: {Assembly}", assemblyGroup.Key);
                var namespaceGroups = assemblyGroup.GroupBy(q => q.Namespace);
                foreach (var namespaceGroup in namespaceGroups)
                {
                    logger.LogInformation("    * Namespace: {Namespace}", namespaceGroup.Key);
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
                        var resultIndicator = query.IsResultType ? " (IResult)" : "";
                        
                        logger.LogInformation("      {StatusIcon} {ClassName}{TypeParameters} : {PrimaryInterface}", statusIcon, query.ClassName, query.TypeParameters, query.PrimaryInterface);
                        logger.LogInformation("        │ Type:        {FullTypeName}", query.Type.FullName);
                        logger.LogInformation("        │ Returns:     {ResponseType}{ResultIndicator}", responseType, resultIndicator);
                        logger.LogInformation("        │ Handler:     {HandlerDetails}", query.HandlerDetails);
                        logger.LogInformation("        │ Status:      {HandlerStatus}", query.HandlerStatus);
                        logger.LogInformation("        │ Assembly:    {Assembly}", query.Assembly);
                        logger.LogInformation("        │ Namespace:   {Namespace}", query.Namespace);
                        
                        if (query.Handlers.Count > 1)
                        {
                            logger.LogInformation("        │ All Types:   [{HandlerTypes}]", string.Join(", ", query.Handlers.Select(h => h.Name)));
                        }
                        if (query.Handlers.Count > 0)
                        {
                            logger.LogInformation("        │ Handler(s):  {HandlerCount} registered", query.Handlers.Count);
                        }
                        logger.LogInformation("        └─ Result Type: {IsResult}", query.IsResultType ? "YES (implements IResult)" : "NO (standard type)");
                        logger.LogInformation("");
                    }
                }
            }
        }
        else
        {
            logger.LogInformation("  (No queries discovered)");
        }

        var commands = mediatorStatistics.AnalyzeCommands(serviceProvider);
        logger.LogInformation("");
        logger.LogInformation($"* {commands.Count} COMMANDS DISCOVERED:");
        if (commands.Any())
        {
            var commandGroups = commands.GroupBy(c => c.Assembly);
            foreach (var assemblyGroup in commandGroups)
            {
                logger.LogInformation("  * Assembly: {Assembly}", assemblyGroup.Key);
                var namespaceGroups = assemblyGroup.GroupBy(c => c.Namespace);
                foreach (var namespaceGroup in namespaceGroups)
                {
                    logger.LogInformation("    * Namespace: {Namespace}", namespaceGroup.Key);
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
                        var resultIndicator = command.IsResultType ? " (IResult)" : "";
                        
                        logger.LogInformation("      {StatusIcon} {ClassName}{TypeParameters} : {PrimaryInterface}", statusIcon, command.ClassName, command.TypeParameters, command.PrimaryInterface);
                        logger.LogInformation("        │ Type:        {FullTypeName}", command.Type.FullName);
                        logger.LogInformation("        │ Returns:     {ResponseType}{ResultIndicator}", responseType, resultIndicator);
                        logger.LogInformation("        │ Handler:     {HandlerDetails}", command.HandlerDetails);
                        logger.LogInformation("        │ Status:      {HandlerStatus}", command.HandlerStatus);
                        logger.LogInformation("        │ Assembly:    {Assembly}", command.Assembly);
                        logger.LogInformation("        │ Namespace:   {Namespace}", command.Namespace);
                        
                        if (command.Handlers.Count > 1)
                        {
                            logger.LogInformation("        │ All Types:   [{HandlerTypes}]", string.Join(", ", command.Handlers.Select(h => h.Name)));
                        }
                        if (command.Handlers.Count > 0)
                        {
                            logger.LogInformation("        │ Handler(s):  {HandlerCount} registered", command.Handlers.Count);
                        }
                        logger.LogInformation("        └─ Result Type: {IsResult}", command.IsResultType ? "YES (implements IResult)" : "NO (standard type)");
                        logger.LogInformation("");
                    }
                }
            }
        }
        else
        {
            logger.LogInformation("  (No commands discovered)");
        }
        
        logger.LogInformation("");
        logger.LogInformation("LEGEND:");
        logger.LogInformation("  + = Handler found (Single)    ! = No handler (Missing)    # = Multiple handlers");
        logger.LogInformation("  │ = Property details          └─ = Additional information");
        logger.LogInformation("===============================================");
        logger.LogInformation("");
    }

    /// <summary>
    /// Orchestrates the complete e-commerce demo workflow.
    /// </summary>
    public async Task Run()
    {
        logger.LogInformation("Starting E-Commerce Demo with Blazing.Mediator...");

        // First, analyze the application structure
        InspectMediatorTypes();

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
