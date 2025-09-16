using TypedMiddlewareExample.Commands;
using TypedMiddlewareExample.Queries;

namespace TypedMiddlewareExample.Services;

/// <summary>
/// Service responsible for running the TypedMiddlewareExample demonstration.
/// Shows the distinction between ICommand and IQuery processing with validation middleware.
/// </summary>
public class Runner
{
    private readonly IMediator _mediator;
    private readonly ILogger<Runner> _logger;
    private readonly IMiddlewarePipelineInspector _pipelineInspector;
    private readonly IServiceProvider _serviceProvider;
    private readonly MediatorStatistics _mediatorStatistics;

    public Runner(
        IMediator mediator,
        ILogger<Runner> logger,
        IMiddlewarePipelineInspector pipelineInspector,
        IServiceProvider serviceProvider,
        MediatorStatistics mediatorStatistics)
    {
        _mediator = mediator;
        _logger = logger;
        _pipelineInspector = pipelineInspector;
        _serviceProvider = serviceProvider;
        _mediatorStatistics = mediatorStatistics;
    }

    /// <summary>
    /// Inspects and displays the middleware pipeline configuration.
    /// </summary>
    public void InspectMiddlewarePipeline()
    {
        var middlewareAnalysis = MiddlewarePipelineAnalyzer.AnalyzeMiddleware(_pipelineInspector, _serviceProvider);

        Console.WriteLine("Registered middleware:");
        foreach (var middleware in middlewareAnalysis)
        {
            Console.WriteLine($"  - [{middleware.OrderDisplay}] {middleware.ClassName}{middleware.TypeParameters}");
            if (!string.IsNullOrEmpty(middleware.GenericConstraints))
            {
                Console.WriteLine($"        - Constraints: {middleware.GenericConstraints}");
            }
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Inspects and displays the mediator types (queries and commands).
    /// </summary>
    public void InspectMediatorTypes()
    {
        Console.WriteLine("=== COMPREHENSIVE MEDIATOR ANALYSIS ===");
        Console.WriteLine();

        // Show compact mode first
        Console.WriteLine("COMPACT MODE (isDetailed: false):");
        Console.WriteLine("══════════════════════════════════════");
        var compactQueries = _mediatorStatistics.AnalyzeQueries(_serviceProvider, isDetailed: false);
        var compactCommands = _mediatorStatistics.AnalyzeCommands(_serviceProvider, isDetailed: false);

        DisplayAnalysisResults("QUERIES", compactQueries, isDetailed: false);
        DisplayAnalysisResults("COMMANDS", compactCommands, isDetailed: false);

        Console.WriteLine();
        Console.WriteLine("LEGEND: + = Handler found, ! = No handler, # = Multiple handlers");
        Console.WriteLine();

        // Show detailed mode
        Console.WriteLine("DETAILED MODE (isDetailed: true - Default):");
        Console.WriteLine("════════════════════════════════════════════════");
        var detailedQueries = _mediatorStatistics.AnalyzeQueries(_serviceProvider, isDetailed: true);
        var detailedCommands = _mediatorStatistics.AnalyzeCommands(_serviceProvider, isDetailed: true);

        DisplayAnalysisResults("QUERIES", detailedQueries, isDetailed: true);
        DisplayAnalysisResults("COMMANDS", detailedCommands, isDetailed: true);

        Console.WriteLine();
        Console.WriteLine("LEGEND:");
        Console.WriteLine("  + = Handler found (Single)    ! = No handler (Missing)    # = Multiple handlers");
        Console.WriteLine("  │ = Property details          └─ = Additional information");
        Console.WriteLine("===============================================");
        Console.WriteLine();
    }

    /// <summary>
    /// Runs the complete demonstration showing ICommand vs IQuery processing.
    /// </summary>
    public async Task Run()
    {
        _logger.LogInformation("Starting TypedMiddlewareExample Demo with Blazing.Mediator...");
        Console.WriteLine();

        // Test constraint display
        ConstraintTestRunner.TestConstraintDisplay(_serviceProvider);

        // Inspect middleware pipeline configuration first
        InspectMiddlewarePipeline();

        // Inspect mediator types before running examples
        InspectMediatorTypes();

        // Demonstrate each type of operation
        await DemonstrateProductLookup();
        await DemonstrateInventoryManagement();
        await DemonstrateOrderConfirmation();
        await DemonstrateCustomerRegistration();
        await DemonstrateCustomerDetailsUpdate();

        // Show execution statistics
        Console.WriteLine("=== EXECUTION STATISTICS ===");
        _mediatorStatistics.ReportStatistics();
        Console.WriteLine("=============================");
        Console.WriteLine();

        _logger.LogInformation("TypedMiddlewareExample Demo completed!");
    }

    /// <summary>
    /// Demonstrates product lookup using IQuery (no validation).
    /// </summary>
    private async Task DemonstrateProductLookup()
    {
        Console.WriteLine("-------- PRODUCT LOOKUP (IQuery) --------");
        _logger.LogDebug(">> Looking up product: WIDGET-001");

        var query = new GetProductQuery { ProductId = "WIDGET-001" };
        var result = await _mediator.Send(query);

        _logger.LogDebug("<< Product found: {Result}", result);
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates inventory management using ICommand<T> (with validation).
    /// </summary>
    private async Task DemonstrateInventoryManagement()
    {
        Console.WriteLine("-------- INVENTORY MANAGEMENT (ICommand<T>) --------");
        _logger.LogDebug(">> Updating inventory for: WIDGET-001, change: -5");

        var command = new UpdateInventoryCommand { ProductId = "WIDGET-001", InventoryChange = -5 };
        var newCount = await _mediator.Send(command);

        _logger.LogDebug("<< New stock count: {NewCount} units", newCount);
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates order confirmation using ICommand (with validation).
    /// </summary>
    private async Task DemonstrateOrderConfirmation()
    {
        Console.WriteLine("-------- ORDER CONFIRMATION (ICommand) --------");
        _logger.LogDebug(">> Sending order confirmation for: ORD-2025-001 to: customer@example.com");

        var command = new SendOrderConfirmationCommand
        {
            OrderId = "ORD-2025-001",
            CustomerEmail = "customer@example.com"
        };
        await _mediator.Send(command);

        _logger.LogDebug("<< Order confirmation sent successfully!");
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates customer registration with validation failure and success.
    /// </summary>
    private async Task DemonstrateCustomerRegistration()
    {
        Console.WriteLine("-------- CUSTOMER REGISTRATION (ICommand with Validation) --------");

        // First attempt with invalid data (validation should fail)
        _logger.LogDebug(">> Registering customer: J (john.doe@example.com)");

        try
        {
            var invalidCommand = new RegisterCustomerCommand
            {
                FullName = "J",  // Too short - will fail validation
                Email = "john.doe@example.com",
                ContactMethod = "Email"
            };
            await _mediator.Send(invalidCommand);
        }
        catch (ValidationException)
        {
            // Expected - validation middleware caught this
        }

        Console.WriteLine();

        // Second attempt with valid data
        _logger.LogDebug(">> Registering customer: John Doe (john.doe@example.com)");

        var validCommand = new RegisterCustomerCommand
        {
            FullName = "John Doe",  // Valid name
            Email = "john.doe@example.com",
            ContactMethod = "Email"
        };
        await _mediator.Send(validCommand);

        _logger.LogDebug("<< Customer registered successfully!");
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates customer details update with validation failure and retry success.
    /// </summary>
    private async Task DemonstrateCustomerDetailsUpdate()
    {
        Console.WriteLine("-------- CUSTOMER DETAILS UPDATE (ICommand<T> with Validation Error & Retry Success) --------");

        // First attempt with invalid data
        _logger.LogDebug(">> Updating customer details (invalid data): INVALID-ID - John Doe (john.doe@example.com)");

        try
        {
            var invalidCommand = new UpdateCustomerDetailsCommand
            {
                CustomerId = "INVALID-ID",  // Invalid format - will fail validation
                FullName = "John Doe",
                Email = "john.doe@example.com",
                ContactMethod = "Email"
            };
            await _mediator.Send(invalidCommand);
        }
        catch (ValidationException)
        {
            // Expected - validation middleware caught this
        }

        Console.WriteLine();

        // Second attempt with valid data
        _logger.LogDebug(">> Updating customer details (valid data): CUST-123456 - John Doe (john.doe@example.com)");

        var validCommand = new UpdateCustomerDetailsCommand
        {
            CustomerId = "CUST-123456",  // Valid format
            FullName = "John Doe",
            Email = "john.doe@example.com",
            ContactMethod = "Email"
        };
        var success = await _mediator.Send(validCommand);

        _logger.LogDebug("<< Customer details updated successfully!");
        Console.WriteLine();
    }

    /// <summary>
    /// Helper method to display analysis results.
    /// </summary>
    private static void DisplayAnalysisResults(string type, IReadOnlyList<QueryCommandAnalysis> results, bool isDetailed)
    {
        Console.WriteLine($"* {results.Count} {type} DISCOVERED:");

        if (results.Count == 0)
        {
            Console.WriteLine("  (None found)");
            return;
        }

        var groupedResults = results.GroupBy(r => r.Assembly)
            .OrderBy(g => g.Key);

        foreach (var assemblyGroup in groupedResults)
        {
            Console.WriteLine($"  * Assembly: {assemblyGroup.Key}");

            var namespaceGroups = assemblyGroup.GroupBy(r => r.Namespace)
                .OrderBy(g => g.Key);

            foreach (var namespaceGroup in namespaceGroups)
            {
                Console.WriteLine($"    * Namespace: {namespaceGroup.Key}");

                var orderedItems = namespaceGroup.OrderBy(r => r.ClassName);


                foreach (var item in orderedItems)
                {
                    var statusIcon = item.HandlerStatus switch
                    {
                        HandlerStatus.Single => "+",
                        HandlerStatus.Missing => "!",
                        HandlerStatus.Multiple => "#",
                        _ => "?"
                    };

                    Console.WriteLine($"      {statusIcon} {item.ClassName} : {item.PrimaryInterface}");

                    if (isDetailed)
                    {
                        Console.WriteLine($"        │ Type:        {item.Type?.FullName}");
                        Console.WriteLine($"        │ Returns:     {item.ResponseType}");
                        Console.WriteLine($"        │ Handler:     {(item.Handlers.Any() ? string.Join(", ", item.Handlers.Select(h => h.Name.Replace("Handler", ""))) : "None")}");
                        Console.WriteLine($"        │ Status:      {item.HandlerStatus}");
                        Console.WriteLine($"        │ Assembly:    {item.Assembly}");
                        Console.WriteLine($"        │ Namespace:   {item.Namespace}");
                        Console.WriteLine($"        │ Handler(s):  {item.Handlers.Count} registered");
                        Console.WriteLine($"        └─ Result Type: {(item.IsResultType ? "YES (IResult)" : "NO (standard type)")}");
                    }
                }
            }
        }

        Console.WriteLine();
    }
}