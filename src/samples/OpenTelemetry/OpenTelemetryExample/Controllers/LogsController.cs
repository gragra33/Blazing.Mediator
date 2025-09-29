using Blazing.Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Application.Queries;
using OpenTelemetryExample.Application.Services;
using OpenTelemetryExample.Domain.Entities;
using OpenTelemetryExample.Infrastructure.Data;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Controllers;

/// <summary>
/// Controller for telemetry logs API endpoints.
/// Provides access to captured log entries with filtering and pagination capabilities.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LogsController(IMediator mediator, ILogger<LogsController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves recent telemetry logs with optional filtering and pagination.
    /// </summary>
    /// <param name="timeWindowMinutes">Time window in minutes to look back (default: 30).</param>
    /// <param name="appOnly">Filter to show only application logs.</param>
    /// <param name="mediatorOnly">Filter to show only Mediator logs.</param>
    /// <param name="errorsOnly">Filter to show only error logs.</param>
    /// <param name="minLogLevel">Minimum log level to include.</param>
    /// <param name="searchText">Text to search for in log messages.</param>
    /// <param name="page">Page number for pagination (default: 1).</param>
    /// <param name="pageSize">Number of items per page (default: 20, max: 100).</param>
    /// <returns>A paginated list of recent logs matching the specified criteria.</returns>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(RecentLogsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RecentLogsDto>> GetRecentLogs(
        [FromQuery] int timeWindowMinutes = 30,
        [FromQuery] bool appOnly = false,
        [FromQuery] bool mediatorOnly = false,
        [FromQuery] bool errorsOnly = false,
        [FromQuery] string? minLogLevel = null,
        [FromQuery] string? searchText = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            // Validate parameters
            if (timeWindowMinutes <= 0 || timeWindowMinutes > 1440) // Max 24 hours
            {
                return BadRequest("Time window must be between 1 and 1440 minutes");
            }

            if (page < 1)
            {
                return BadRequest("Page number must be greater than 0");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100");
            }

            // Validate log level if provided
            if (!string.IsNullOrEmpty(minLogLevel))
            {
                var validLogLevels = new[] { "Trace", "Debug", "Information", "Warning", "Error", "Critical" };
                if (!validLogLevels.Contains(minLogLevel, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest($"Invalid log level. Valid values are: {string.Join(", ", validLogLevels)}");
                }
            }

            logger.LogDebug("Getting recent logs with parameters: TimeWindow={TimeWindow}, AppOnly={AppOnly}, MediatorOnly={MediatorOnly}, ErrorsOnly={ErrorsOnly}, MinLevel={MinLevel}, Search='{Search}', Page={Page}, PageSize={PageSize}",
                timeWindowMinutes, appOnly, mediatorOnly, errorsOnly, minLogLevel, searchText, page, pageSize);

            var query = new GetRecentLogsQuery(
                timeWindowMinutes,
                appOnly,
                mediatorOnly,
                errorsOnly,
                minLogLevel,
                searchText,
                page,
                pageSize);

            var result = await mediator.Send(query).ConfigureAwait(false);

            logger.LogDebug("Successfully retrieved {LogCount} logs for page {Page} of {TotalPages}",
                result.Logs.Count(), page, result.Pagination.TotalPages);

            return Ok(result);
        }
        catch (Exception ex)
        {
            LogsControllerLog.LogErrorRetrievingRecentLogs(logger, ex);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving logs");
        }
    }

    /// <summary>
    /// Retrieves detailed information about a specific log entry.
    /// </summary>
    /// <param name="id">The ID of the log entry to retrieve.</param>
    /// <returns>Detailed log information.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(LogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LogDto>> GetLogById(int id)
    {
        try
        {
            logger.LogDebug("Getting log details for ID: {LogId}", id);

            // For now, we'll use the recent logs query with a filter for the specific ID
            // In a real implementation, you might want a dedicated query for this
            var query = new GetRecentLogsQuery(TimeWindowMinutes: 1440); // 24 hours
            var result = await mediator.Send(query).ConfigureAwait(false);

            var log = result.Logs.FirstOrDefault(l => l.Id == id);
            if (log == null)
            {
                logger.LogWarning("Log with ID {LogId} not found", id);
                return NotFound($"Log with ID {id} not found");
            }

            logger.LogDebug("Successfully retrieved log details for ID: {LogId}", id);
            return Ok(log);
        }
        catch (Exception ex)
        {
            LogsControllerLog.LogErrorRetrievingLogById(logger, ex, id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the log");
        }
    }

    /// <summary>
    /// Gets summary statistics for logs within the specified time window.
    /// </summary>
    /// <param name="timeWindowMinutes">Time window in minutes to analyze (default: 30).</param>
    /// <returns>Summary statistics for the logs in the time window.</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(LogSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LogSummary>> GetLogsSummary([FromQuery] int timeWindowMinutes = 30)
    {
        try
        {
            if (timeWindowMinutes <= 0 || timeWindowMinutes > 1440)
            {
                return BadRequest("Time window must be between 1 and 1440 minutes");
            }

            logger.LogDebug("Getting logs summary for time window: {TimeWindow} minutes", timeWindowMinutes);

            var query = new GetRecentLogsQuery(TimeWindowMinutes: timeWindowMinutes, PageSize: 1); // We only need the summary
            var result = await mediator.Send(query).ConfigureAwait(false);

            logger.LogDebug("Successfully retrieved logs summary");
            return Ok(result.Summary);
        }
        catch (Exception ex)
        {
            LogsControllerLog.LogErrorRetrievingLogsSummary(logger, ex);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving logs summary");
        }
    }

    /// <summary>
    /// Test endpoint to generate sample logs for telemetry testing.
    /// </summary>
    /// <returns>Test result with log generation status.</returns>
    [HttpPost("test-logging")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> TestLogging()
    {
        logger.LogInformation("Testing logging functionality - generating sample logs");

        try
        {
            // Test that we can get the TelemetryDatabaseLoggingProvider
            var loggingProvider = HttpContext.RequestServices.GetService<TelemetryDatabaseLoggingProvider>();
            var providerFound = loggingProvider != null;

            logger.LogInformation("TelemetryDatabaseLoggingProvider found in DI: {ProviderFound}", providerFound);

            // Generate various types of logs for testing
            logger.LogDebug("Debug log: Starting test logging process");
            logger.LogInformation("Information log: Processing test logging request");
            logger.LogWarning("Warning log: This is a test warning message");

            // Simulate some work
            await Task.Delay(100).ConfigureAwait(false);

            // Log with structured data
            logger.LogInformation("Structured log: User {UserId} performed action {Action} at {Timestamp}",
                123, "TestLogging", DateTime.UtcNow);

            // Simulate an error scenario (but catch it)
            try
            {
                throw new InvalidOperationException("Test exception for logging demonstration");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error log: Caught test exception during logging test");
            }

            logger.LogInformation("Test logging completed successfully");

            // Wait a moment for logs to be processed
            await Task.Delay(3000).ConfigureAwait(false);

            // Check if logs were actually saved to database
            using var scope = HttpContext.RequestServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var recentLogs = await context.TelemetryLogs
                .Where(l => l.Category.Contains("LogsController"))
                .OrderByDescending(l => l.Timestamp)
                .Take(10)
                .ToListAsync().ConfigureAwait(false);

            return Ok(new
            {
                message = "Test logs generated successfully",
                timestamp = DateTime.UtcNow,
                generatedLogs = 6,
                telemetryProviderFound = providerFound,
                recentLogsCaptured = recentLogs.Count,
                sampleLogs = recentLogs.Select(l => new { l.LogLevel, l.Message, l.Timestamp }).ToList()
            });
        }
        catch (Exception ex)
        {
            LogsControllerLog.LogUnexpectedErrorDuringTestLogging(logger, ex);
            return StatusCode(500, "Error occurred during test logging");
        }
    }

    /// <summary>
    /// Test endpoint to directly save a log to the database (bypassing the logging provider).
    /// </summary>
    /// <returns>Test result with database save status.</returns>
    [HttpPost("test-database-logging")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> TestDatabaseLogging()
    {
        logger.LogInformation("Testing direct database logging");

        try
        {
            using var scope = HttpContext.RequestServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var testLog = new TelemetryLog
            {
                Timestamp = DateTime.UtcNow,
                LogLevel = "Information",
                Category = "OpenTelemetryExample.Controllers.LogsController",
                Message = "Direct database test log entry",
                Source = "Controller",
                MachineName = Environment.MachineName,
                ProcessId = Environment.ProcessId,
                ThreadId = Environment.CurrentManagedThreadId,
                Tags = new Dictionary<string, object> { { "test", "direct-save" } }
            };

            context.TelemetryLogs.Add(testLog);
            var savedCount = await context.SaveChangesAsync().ConfigureAwait(false);

            logger.LogInformation("Direct database save completed - saved {SavedCount} log entries", savedCount);

            // Check if the log was actually saved
            var totalLogs = await context.TelemetryLogs.CountAsync().ConfigureAwait(false);

            return Ok(new
            {
                message = "Direct database test completed",
                savedCount,
                totalLogsInDatabase = totalLogs,
                testLog = new { testLog.Id, testLog.Timestamp, testLog.Message }
            });
        }
        catch (Exception ex)
        {
            LogsControllerLog.LogErrorDuringDirectDatabaseLoggingTest(logger, ex);
            return StatusCode(500, new
            {
                message = "Error during direct database test",
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }
}
