using System.Collections.Generic;

namespace Blazing.Mediator.SourceGenerators.Models;

/// <summary>
/// Aggregate compilation model fed into <see cref="Emitters.MediatorCodeWriter"/>.
/// Contains every piece of information discovered by <c>CompilationAnalyzer</c>
/// that is required to emit the complete <c>Mediator.g.cs</c>.
/// </summary>
internal sealed class CompilationModel
{
    public CompilationModel(
        string rootNamespace,
        List<RequestHandlerModel> requests,
        List<NotificationHandlerModel> notifications,
        List<MiddlewareModel> middleware)
    {
        RootNamespace = rootNamespace;
        Requests = requests;
        Notifications = notifications;
        Middleware = middleware;
    }

    /// <summary>Root namespace of the consuming assembly.</summary>
    public string RootNamespace { get; }

    /// <summary>All discovered request/command/query and stream handlers.</summary>
    public List<RequestHandlerModel> Requests { get; }

    /// <summary>All discovered notification types and their handlers.</summary>
    public List<NotificationHandlerModel> Notifications { get; }

    /// <summary>All discovered middleware implementations.</summary>
    public List<MiddlewareModel> Middleware { get; }
}
