using Blazing.Mediator.Statistics;
using Blazing.Mediator.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace UserManagement.Api.Controllers;

/// <summary>
/// Controller for mediator statistics and analysis endpoints in the User Management API.
/// Demonstrates the new MediatorStatistics functionality for inspecting queries and commands.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MediatorAnalysisController : ControllerBase
{
    private readonly MediatorStatistics _mediatorStatistics;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MediatorAnalysisController> _logger;

    /// <summary>
    /// Initializes a new instance of the MediatorAnalysisController.
    /// </summary>
    /// <param name="mediatorStatistics">The mediator statistics service.</param>
    /// <param name="serviceProvider">The service provider for type discovery.</param>
    /// <param name="logger">The logger instance.</param>
    public MediatorAnalysisController(
        MediatorStatistics mediatorStatistics, 
        IServiceProvider serviceProvider,
        ILogger<MediatorAnalysisController> logger)
    {
        _mediatorStatistics = mediatorStatistics;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Gets a health check status and basic mediator information.
    /// </summary>
    /// <returns>API health status</returns>
    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        _logger.LogInformation("Health check requested");
        
        return Ok(new
        {
            Status = "Healthy",
            Service = "User Management API - Mediator Analysis",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0"
        });
    }

    /// <summary>
    /// Gets runtime statistics about mediator usage including query and command execution counts.
    /// </summary>
    /// <returns>Mediator runtime statistics</returns>
    [HttpGet("runtime-statistics")]
    public IActionResult GetRuntimeStatistics()
    {
        _logger.LogInformation("Runtime statistics requested");
        
        // Create a custom statistics renderer that captures output
        var capturedOutput = new List<string>();
        var captureRenderer = new CapturingStatisticsRenderer(capturedOutput);
        
        // Create a temporary statistics instance with our capture renderer to get current stats
        var tempStats = new MediatorStatistics(captureRenderer);
        tempStats.ReportStatistics();

        return Ok(new
        {
            Message = "User Management API - Mediator Runtime Statistics",
            StatisticsOutput = capturedOutput,
            RequestedAt = DateTime.UtcNow,
            Note = "These statistics show runtime execution counts since application startup"
        });
    }

    /// <summary>
    /// Analyzes all user management queries in the application.
    /// </summary>
    /// <returns>Detailed analysis of all discovered queries related to user management</returns>
    [HttpGet("queries")]
    public IActionResult AnalyzeQueries()
    {
        _logger.LogInformation("Query analysis requested");
        
        var queries = _mediatorStatistics.AnalyzeQueries(_serviceProvider);
        
        var result = new
        {
            Analysis = "User Management Queries",
            TotalQueries = queries.Count,
            QueriesByAssembly = queries
                .GroupBy(q => q.Assembly)
                .OrderBy(g => g.Key)
                .Select(assemblyGroup => new
                {
                    Assembly = assemblyGroup.Key,
                    QueryCount = assemblyGroup.Count(),
                    Namespaces = assemblyGroup
                        .GroupBy(q => q.Namespace)
                        .OrderBy(g => g.Key)
                        .Select(namespaceGroup => new
                        {
                            Namespace = namespaceGroup.Key,
                            QueryCount = namespaceGroup.Count(),
                            Queries = namespaceGroup
                                .OrderBy(q => q.ClassName)
                                .Select(q => new
                                {
                                    Name = q.ClassName,
                                    TypeParameters = q.TypeParameters,
                                    ResponseType = q.ResponseType?.Name ?? "void",
                                    ResponseTypeFullName = q.ResponseType?.FullName,
                                    FullTypeName = q.Type.FullName,
                                    HandlerStatus = q.HandlerStatus.ToString(),
                                    HandlerDetails = q.HandlerDetails,
                                    Handlers = q.Handlers.Select(h => h.Name).ToList(),
                                    StatusIcon = q.HandlerStatus switch
                                    {
                                        HandlerStatus.Single => "+",
                                        HandlerStatus.Missing => "!",
                                        HandlerStatus.Multiple => "#",
                                        _ => "?"
                                    }
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .ToList(),
            HealthSummary = new
            {
                WithHandlers = queries.Count(q => q.HandlerStatus == HandlerStatus.Single),
                MissingHandlers = queries.Count(q => q.HandlerStatus == HandlerStatus.Missing),
                MultipleHandlers = queries.Count(q => q.HandlerStatus == HandlerStatus.Multiple)
            },
            Legend = new
            {
                StatusIcons = new { Success = "+", Missing = "!", Multiple = "#" },
                Description = "+ = Handler found, ! = No handler, # = Multiple handlers"
            },
            AnalyzedAt = DateTime.UtcNow
        };

        return Ok(result);
    }

    /// <summary>
    /// Analyzes all user management commands in the application.
    /// </summary>
    /// <returns>Detailed analysis of all discovered commands related to user management</returns>
    [HttpGet("commands")]
    public IActionResult AnalyzeCommands()
    {
        _logger.LogInformation("Command analysis requested");
        
        var commands = _mediatorStatistics.AnalyzeCommands(_serviceProvider);
        
        var result = new
        {
            Analysis = "User Management Commands",
            TotalCommands = commands.Count,
            CommandsByAssembly = commands
                .GroupBy(c => c.Assembly)
                .OrderBy(g => g.Key)
                .Select(assemblyGroup => new
                {
                    Assembly = assemblyGroup.Key,
                    CommandCount = assemblyGroup.Count(),
                    Namespaces = assemblyGroup
                        .GroupBy(c => c.Namespace)
                        .OrderBy(g => g.Key)
                        .Select(namespaceGroup => new
                        {
                            Namespace = namespaceGroup.Key,
                            CommandCount = namespaceGroup.Count(),
                            Commands = namespaceGroup
                                .OrderBy(c => c.ClassName)
                                .Select(c => new
                                {
                                    Name = c.ClassName,
                                    TypeParameters = c.TypeParameters,
                                    ResponseType = c.ResponseType?.Name ?? "void",
                                    ResponseTypeFullName = c.ResponseType?.FullName,
                                    FullTypeName = c.Type.FullName,
                                    HandlerStatus = c.HandlerStatus.ToString(),
                                    HandlerDetails = c.HandlerDetails,
                                    Handlers = c.Handlers.Select(h => h.Name).ToList(),
                                    StatusIcon = c.HandlerStatus switch
                                    {
                                        HandlerStatus.Single => "+",
                                        HandlerStatus.Missing => "!",
                                        HandlerStatus.Multiple => "#",
                                        _ => "?"
                                    }
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .ToList(),
            HealthSummary = new
            {
                WithHandlers = commands.Count(c => c.HandlerStatus == HandlerStatus.Single),
                MissingHandlers = commands.Count(c => c.HandlerStatus == HandlerStatus.Missing),
                MultipleHandlers = commands.Count(c => c.HandlerStatus == HandlerStatus.Multiple)
            },
            Legend = new
            {
                StatusIcons = new { Success = "+", Missing = "!", Multiple = "#" },
                Description = "+ = Handler found, ! = No handler, # = Multiple handlers"
            },
            AnalyzedAt = DateTime.UtcNow
        };

        return Ok(result);
    }

    /// <summary>
    /// Gets a comprehensive analysis report of the entire user management mediator setup.
    /// </summary>
    /// <returns>Complete mediator analysis including queries, commands, and statistics</returns>
    [HttpGet("comprehensive-report")]
    public IActionResult GetComprehensiveReport()
    {
        _logger.LogInformation("Comprehensive analysis report requested");
        
        var queries = _mediatorStatistics.AnalyzeQueries(_serviceProvider);
        var commands = _mediatorStatistics.AnalyzeCommands(_serviceProvider);

        // Group by assembly for better organization
        var assemblies = queries.Select(q => q.Assembly)
            .Union(commands.Select(c => c.Assembly))
            .Distinct()
            .OrderBy(a => a)
            .ToList();

        var assemblyAnalysis = assemblies.Select(assembly => new
        {
            Assembly = assembly,
            Statistics = new
            {
                QueryCount = queries.Count(q => q.Assembly == assembly),
                CommandCount = commands.Count(c => c.Assembly == assembly),
                QueriesWithHandlers = queries.Count(q => q.Assembly == assembly && q.HandlerStatus == HandlerStatus.Single),
                QueriesMissingHandlers = queries.Count(q => q.Assembly == assembly && q.HandlerStatus == HandlerStatus.Missing),
                QueriesWithMultipleHandlers = queries.Count(q => q.Assembly == assembly && q.HandlerStatus == HandlerStatus.Multiple),
                CommandsWithHandlers = commands.Count(c => c.Assembly == assembly && c.HandlerStatus == HandlerStatus.Single),
                CommandsMissingHandlers = commands.Count(c => c.Assembly == assembly && c.HandlerStatus == HandlerStatus.Missing),
                CommandsWithMultipleHandlers = commands.Count(c => c.Assembly == assembly && c.HandlerStatus == HandlerStatus.Multiple)
            },
            Queries = queries
                .Where(q => q.Assembly == assembly)
                .GroupBy(q => q.Namespace)
                .ToDictionary(g => g.Key, g => g.Select(q => new { 
                    q.ClassName, 
                    q.TypeParameters, 
                    ResponseType = q.ResponseType?.Name,
                    HandlerStatus = q.HandlerStatus.ToString(),
                    q.HandlerDetails,
                    StatusIcon = q.HandlerStatus switch
                    {
                        HandlerStatus.Single => "+",
                        HandlerStatus.Missing => "!",
                        HandlerStatus.Multiple => "#",
                        _ => "?"
                    }
                }).ToList()),
            Commands = commands
                .Where(c => c.Assembly == assembly)
                .GroupBy(c => c.Namespace)
                .ToDictionary(g => g.Key, g => g.Select(c => new { 
                    c.ClassName, 
                    c.TypeParameters, 
                    ResponseType = c.ResponseType?.Name,
                    HandlerStatus = c.HandlerStatus.ToString(),
                    c.HandlerDetails,
                    StatusIcon = c.HandlerStatus switch
                    {
                        HandlerStatus.Single => "+",
                        HandlerStatus.Missing => "!",
                        HandlerStatus.Multiple => "#",
                        _ => "?"
                    }
                }).ToList())
        }).ToList();

        return Ok(new
        {
            ReportTitle = "User Management API - Comprehensive Mediator Analysis",
            Summary = new
            {
                TotalQueries = queries.Count,
                TotalCommands = commands.Count,
                TotalTypes = queries.Count + commands.Count,
                AssembliesAnalyzed = assemblies.Count,
                OverallHealth = new
                {
                    QueriesWithHandlers = queries.Count(q => q.HandlerStatus == HandlerStatus.Single),
                    QueriesMissingHandlers = queries.Count(q => q.HandlerStatus == HandlerStatus.Missing),
                    QueriesWithMultipleHandlers = queries.Count(q => q.HandlerStatus == HandlerStatus.Multiple),
                    CommandsWithHandlers = commands.Count(c => c.HandlerStatus == HandlerStatus.Single),
                    CommandsMissingHandlers = commands.Count(c => c.HandlerStatus == HandlerStatus.Missing),
                    CommandsWithMultipleHandlers = commands.Count(c => c.HandlerStatus == HandlerStatus.Multiple)
                }
            },
            AssemblyBreakdown = assemblyAnalysis,
            Legend = new
            {
                StatusIcons = new { Success = "+", Missing = "!", Multiple = "#" },
                Description = "+ = Handler found, ! = No handler, # = Multiple handlers"
            },
            GeneratedAt = DateTime.UtcNow,
            ApiVersion = "1.0.0",
            Note = "This report shows all queries and commands discovered in the application with their handler status"
        });
    }
}

/// <summary>
/// Statistics renderer that captures output to a list for API responses.
/// </summary>
public class CapturingStatisticsRenderer : IStatisticsRenderer
{
    private readonly List<string> _capturedOutput;

    /// <summary>
    /// Initializes a new instance of the CapturingStatisticsRenderer class.
    /// </summary>
    /// <param name="capturedOutput">The list to capture output to.</param>
    public CapturingStatisticsRenderer(List<string> capturedOutput)
    {
        _capturedOutput = capturedOutput;
    }

    /// <summary>
    /// Renders a message by adding it to the captured output list.
    /// </summary>
    /// <param name="message">The message to render.</param>
    public void Render(string message)
    {
        _capturedOutput.Add(message);
    }
}