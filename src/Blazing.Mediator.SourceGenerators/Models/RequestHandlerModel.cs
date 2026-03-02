using System.Collections.Generic;

namespace Blazing.Mediator.SourceGenerators.Models;

/// <summary>
/// Describes a single discovered request/command/query/stream handler at compile time.
/// </summary>
internal sealed class RequestHandlerModel
{
    public RequestHandlerModel(
        string safeName,
        string requestType,
        string responseType,
        string handlerType,
        bool isStream,
        bool isVoid,
        bool isQuery,
        List<MiddlewareModel> applicableMiddleware,
        bool isAmbiguous = false,
        List<string>? allHandlerTypes = null)
    {
        SafeName = safeName;
        RequestType = requestType;
        ResponseType = responseType;
        HandlerType = handlerType;
        IsStream = isStream;
        IsVoid = isVoid;
        IsQuery = isQuery;
        ApplicableMiddleware = applicableMiddleware;
        IsAmbiguous = isAmbiguous;
        AllHandlerTypes = allHandlerTypes ?? new List<string> { handlerType };
    }

    /// <summary>
    /// Identifier-safe name derived from the request type, used as a suffix for generated class names
    /// (e.g. "PingRequest" → "PingRequest", "MyNs.Foo.PingRequest" → "MyNs_Foo_PingRequest").
    /// </summary>
    public string SafeName { get; }

    /// <summary>Fully qualified request type (e.g. "MyApp.PingRequest").</summary>
    public string RequestType { get; }

    /// <summary>
    /// Fully qualified response type (e.g. "MyApp.PingResponse", "Blazing.Mediator.Unit" for void commands).
    /// </summary>
    public string ResponseType { get; }

    /// <summary>Fully qualified handler type.</summary>
    public string HandlerType { get; }

    /// <summary>True when this handler implements IStreamRequestHandler&lt;,&gt;.</summary>
    public bool IsStream { get; }

    /// <summary>True when the handler implements IRequestHandler&lt;TRequest&gt; (no return value / void command).</summary>
    public bool IsVoid { get; }

    /// <summary>True when the request type implements <c>IQuery&lt;TResponse&gt;</c> (read-only query).</summary>
    public bool IsQuery { get; }

    /// <summary>Middleware that applies to this specific request type, ordered by <see cref="MiddlewareModel.Order"/>.</summary>
    public List<MiddlewareModel> ApplicableMiddleware { get; }

    /// <summary>
    /// True when more than one handler was discovered for this request type.
    /// The generated dispatcher will emit a <c>ThrowHelper.ThrowMultipleHandlers*()</c> call
    /// instead of a wrapper dispatch, causing an <see cref="System.InvalidOperationException"/> at runtime.
    /// </summary>
    public bool IsAmbiguous { get; }

    /// <summary>
    /// All discovered handler types for this request type.
    /// Contains one entry for non-ambiguous requests; two or more for ambiguous ones.
    /// All are registered with the DI container so the error message can list them.
    /// </summary>
    public List<string> AllHandlerTypes { get; }
}
