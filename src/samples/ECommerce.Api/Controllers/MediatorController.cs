using Blazing.Mediator.Statistics;
using Blazing.Mediator.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

/// <summary>
/// Controller for mediator statistics and analysis endpoints.
/// Demonstrates the new MediatorStatistics functionality for inspecting queries and commands.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MediatorController : ControllerBase
{
    private readonly MediatorStatistics _mediatorStatistics;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the MediatorController.
    /// </summary>
    /// <param name="mediatorStatistics">The mediator statistics service.</param>
    /// <param name="serviceProvider">The service provider for type discovery.</param>
    public MediatorController(MediatorStatistics mediatorStatistics, IServiceProvider serviceProvider)
    {
        _mediatorStatistics = mediatorStatistics;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets statistics about mediator usage including query and command counts.
    /// </summary>
    /// <returns>Mediator usage statistics</returns>
    [HttpGet("statistics")]
    public IActionResult GetStatistics()
    {
        // Create a custom statistics renderer that captures output
        var capturedOutput = new List<string>();
        var captureRenderer = new CapturingStatisticsRenderer(capturedOutput);
        
        // Create a temporary statistics instance with our capture renderer
        var tempStats = new MediatorStatistics(captureRenderer);
        tempStats.ReportStatistics();

        return Ok(new
        {
            Message = "Mediator Statistics",
            Output = capturedOutput,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Analyzes all queries in the application grouped by assembly and namespace.
    /// </summary>
    /// <param name="detailed">If true, returns comprehensive analysis with all properties. If false, returns compact analysis with basic information only.</param>
    /// <returns>Detailed analysis of all discovered queries</returns>
    [HttpGet("analyze/queries")]
    public IActionResult AnalyzeQueries([FromQuery] bool detailed = true)
    {
        var queries = _mediatorStatistics.AnalyzeQueries(_serviceProvider, detailed);
        
        var result = queries
            .GroupBy(q => q.Assembly)
            .OrderBy(g => g.Key)
            .Select(assemblyGroup => new
            {
                Assembly = assemblyGroup.Key,
                Namespaces = assemblyGroup
                    .GroupBy(q => q.Namespace)
                    .OrderBy(g => g.Key)
                    .Select(namespaceGroup => new
                    {
                        Namespace = namespaceGroup.Key,
                        Queries = namespaceGroup
                            .OrderBy(q => q.ClassName)
                            .Select(q => new
                            {
                                q.ClassName,
                                q.TypeParameters,
                                PrimaryInterface = q.PrimaryInterface,
                                ResponseType = q.ResponseType?.Name,
                                IsResultType = q.IsResultType,
                                FullTypeName = q.Type.FullName,
                                HandlerStatus = q.HandlerStatus.ToString(),
                                q.HandlerDetails,
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
            .ToList();

        return Ok(new
        {
            TotalQueries = queries.Count,
            IsDetailed = detailed,
            QueriesByAssembly = result,
            Summary = new
            {
                WithHandlers = queries.Count(q => q.HandlerStatus == HandlerStatus.Single),
                MissingHandlers = queries.Count(q => q.HandlerStatus == HandlerStatus.Missing),
                MultipleHandlers = queries.Count(q => q.HandlerStatus == HandlerStatus.Multiple)
            },
            Legend = new
            {
                Symbols = new { Success = "+", Missing = "!", Multiple = "#" },
                Description = "+ = Handler found, ! = No handler, # = Multiple handlers"
            },
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Analyzes all commands in the application grouped by assembly and namespace.
    /// </summary>
    /// <param name="detailed">If true, returns comprehensive analysis with all properties. If false, returns compact analysis with basic information only.</param>
    /// <returns>Detailed analysis of all discovered commands</returns>
    [HttpGet("analyze/commands")]
    public IActionResult AnalyzeCommands([FromQuery] bool detailed = true)
    {
        var commands = _mediatorStatistics.AnalyzeCommands(_serviceProvider, detailed);
        
        var result = commands
            .GroupBy(c => c.Assembly)
            .OrderBy(g => g.Key)
            .Select(assemblyGroup => new
            {
                Assembly = assemblyGroup.Key,
                Namespaces = assemblyGroup
                    .GroupBy(c => c.Namespace)
                    .OrderBy(g => g.Key)
                    .Select(namespaceGroup => new
                    {
                        Namespace = namespaceGroup.Key,
                        Commands = namespaceGroup
                            .OrderBy(c => c.ClassName)
                            .Select(c => new
                            {
                                c.ClassName,
                                c.TypeParameters,
                                PrimaryInterface = c.PrimaryInterface,
                                ResponseType = c.ResponseType?.Name,
                                IsResultType = c.IsResultType,
                                FullTypeName = c.Type.FullName,
                                HandlerStatus = c.HandlerStatus.ToString(),
                                c.HandlerDetails,
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
            .ToList();

        return Ok(new
        {
            TotalCommands = commands.Count,
            IsDetailed = detailed,
            CommandsByAssembly = result,
            Summary = new
            {
                WithHandlers = commands.Count(c => c.HandlerStatus == HandlerStatus.Single),
                MissingHandlers = commands.Count(c => c.HandlerStatus == HandlerStatus.Missing),
                MultipleHandlers = commands.Count(c => c.HandlerStatus == HandlerStatus.Multiple)
            },
            Legend = new
            {
                Symbols = new { Success = "+", Missing = "!", Multiple = "#" },
                Description = "+ = Handler found, ! = No handler, # = Multiple handlers"
            },
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Gets a comprehensive analysis of both queries and commands.
    /// </summary>
    /// <param name="detailed">If true, returns comprehensive analysis with all properties. If false, returns compact analysis with basic information only.</param>
    /// <returns>Complete mediator analysis including queries, commands, and statistics</returns>
    [HttpGet("analyze")]
    public IActionResult GetCompleteAnalysis([FromQuery] bool detailed = true)
    {
        var queries = _mediatorStatistics.AnalyzeQueries(_serviceProvider, detailed);
        var commands = _mediatorStatistics.AnalyzeCommands(_serviceProvider, detailed);

        return Ok(new
        {
            Summary = new
            {
                TotalQueries = queries.Count,
                TotalCommands = commands.Count,
                TotalTypes = queries.Count + commands.Count,
                IsDetailed = detailed,
                HealthStatus = new
                {
                    QueriesWithHandlers = queries.Count(q => q.HandlerStatus == HandlerStatus.Single),
                    QueriesMissingHandlers = queries.Count(q => q.HandlerStatus == HandlerStatus.Missing),
                    QueriesWithMultipleHandlers = queries.Count(q => q.HandlerStatus == HandlerStatus.Multiple),
                    CommandsWithHandlers = commands.Count(c => c.HandlerStatus == HandlerStatus.Single),
                    CommandsMissingHandlers = commands.Count(c => c.HandlerStatus == HandlerStatus.Missing),
                    CommandsWithMultipleHandlers = commands.Count(c => c.HandlerStatus == HandlerStatus.Multiple)
                }
            },
            Queries = queries
                .GroupBy(q => q.Assembly)
                .ToDictionary(g => g.Key, g => g.GroupBy(q => q.Namespace).ToDictionary(n => n.Key, n => n.Select(q => new
                {
                    q.ClassName,
                    q.TypeParameters,
                    PrimaryInterface = q.PrimaryInterface,
                    ResponseType = q.ResponseType?.Name,
                    IsResultType = q.IsResultType,
                    HandlerStatus = q.HandlerStatus.ToString(),
                    q.HandlerDetails,
                    StatusIcon = q.HandlerStatus switch
                    {
                        HandlerStatus.Single => "+",
                        HandlerStatus.Missing => "!",
                        HandlerStatus.Multiple => "#",
                        _ => "?"
                    }
                }).ToList())),
            Commands = commands
                .GroupBy(c => c.Assembly)
                .ToDictionary(g => g.Key, g => g.GroupBy(c => c.Namespace).ToDictionary(n => n.Key, n => n.Select(c => new
                {
                    c.ClassName,
                    c.TypeParameters,
                    PrimaryInterface = c.PrimaryInterface,
                    ResponseType = c.ResponseType?.Name,
                    IsResultType = c.IsResultType,
                    HandlerStatus = c.HandlerStatus.ToString(),
                    c.HandlerDetails,
                    StatusIcon = c.HandlerStatus switch
                    {
                        HandlerStatus.Single => "+",
                        HandlerStatus.Missing => "!",
                        HandlerStatus.Multiple => "#",
                        _ => "?"
                    }
                }).ToList())),
            Legend = new
            {
                Symbols = new { Success = "+", Missing = "!", Multiple = "#" },
                Description = "+ = Handler found, ! = No handler, # = Multiple handlers"
            },
            Timestamp = DateTime.UtcNow
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