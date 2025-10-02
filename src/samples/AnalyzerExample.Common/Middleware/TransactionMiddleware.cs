using AnalyzerExample.Common.Interfaces;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Common.Middleware;

public class TransactionMiddleware<TRequest> : IRequestMiddleware<TRequest>
    where TRequest : ITransactionalCommand
{
    public static int Order => 100;
    
    private readonly ILogger<TransactionMiddleware<TRequest>> _logger;

    public TransactionMiddleware(ILogger<TransactionMiddleware<TRequest>> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        if (!request.RequiresTransaction)
        {
            await next();
            return;
        }

        _logger.LogInformation("?? [Transaction] Starting transaction for {CommandType}", typeof(TRequest).Name);
        
        // In a real application, you would start a database transaction here
        try
        {
            await next();
            _logger.LogInformation("?? [Transaction] Committed transaction for {CommandType}", typeof(TRequest).Name);
        }
        catch
        {
            _logger.LogWarning("?? [Transaction] Rolling back transaction for {CommandType}", typeof(TRequest).Name);
            throw;
        }
    }
}