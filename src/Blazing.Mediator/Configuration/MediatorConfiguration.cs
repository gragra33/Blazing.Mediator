namespace Blazing.Mediator.Configuration;

/// <summary>
/// Configuration for the mediator, including middleware pipeline setup.
/// </summary>
public class MediatorConfiguration
{
    /// <summary>
    /// Gets the middleware pipeline builder.
    /// </summary>
    public IMiddlewarePipelineBuilder PipelineBuilder { get; } = new MiddlewarePipelineBuilder();

    /// <summary>
    /// Adds a middleware to the pipeline.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware type</typeparam>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration AddMiddleware<TMiddleware>()
        where TMiddleware : class
    {
        PipelineBuilder.AddMiddleware<TMiddleware>();
        return this;
    }

    /// <summary>
    /// Adds an open generic middleware type to the pipeline.
    /// </summary>
    /// <param name="middlewareType">The open generic middleware type</param>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration AddMiddleware(Type middlewareType)
    {
        PipelineBuilder.AddMiddleware(middlewareType);
        return this;
    }
}
