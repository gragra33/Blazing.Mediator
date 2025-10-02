using AnalyzerExample.Common.Interfaces;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Common.Middleware;

public class AuditMiddleware<TRequest> : IRequestMiddleware<TRequest>
    where TRequest : IAuditableCommand
{
    public static int Order => 500;
    
    private readonly ILogger<AuditMiddleware<TRequest>> _logger;

    public AuditMiddleware(ILogger<AuditMiddleware<TRequest>> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("?? [Audit] Command: {CommandType}, User: {UserId}, Reason: {Reason}",
            typeof(TRequest).Name, request.AuditUserId, request.AuditReason);
            
        await next();
        
        _logger.LogInformation("?? [Audit] Completed {CommandType} for user {UserId}",
            typeof(TRequest).Name, request.AuditUserId);
    }
}