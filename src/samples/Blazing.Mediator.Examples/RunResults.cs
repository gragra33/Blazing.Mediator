namespace Blazing.Mediator.Examples;

/// <summary>
/// Results of running the Blazing.Mediator examples.
/// This class tracks which features were successfully demonstrated.
/// Compare with MediatR version: identical structure, just different naming for middleware vs pipeline behaviors.
/// </summary>
public class RunResults
{
    /// <summary>
    /// Gets or sets a value indicating whether request handlers worked.
    /// </summary>
    public bool RequestHandlers { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether void request handlers worked.
    /// </summary>
    public bool VoidRequestsHandlers { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether middleware behaviors worked.
    /// </summary>
    public bool MiddlewareBehaviors { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether request pre-processors worked.
    /// </summary>
    public bool RequestPreProcessors { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether request post-processors worked.
    /// </summary>
    public bool RequestPostProcessors { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether ordered middleware behaviors worked.
    /// </summary>
    public bool OrderedMiddlewareBehaviors { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether constrained generic behaviors worked.
    /// </summary>
    public bool ConstrainedGenericBehaviors { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether notification handlers worked.
    /// </summary>
    public bool NotificationHandler { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether multiple notification handlers worked.
    /// </summary>
    public bool MultipleNotificationHandlers { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether covariant notification handlers worked.
    /// </summary>
    public bool CovariantNotificationHandler { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether constrained generic notification handlers worked.
    /// </summary>
    public bool ConstrainedGenericNotificationHandler { get; set; }

    // Stream results
    /// <summary>
    /// Gets or sets a value indicating whether stream request handlers worked.
    /// </summary>
    public bool StreamRequestHandlers { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether stream middleware behaviors worked.
    /// </summary>
    public bool StreamMiddlewareBehaviors { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether stream ordered middleware behaviors worked.
    /// </summary>
    public bool StreamOrderedMiddlewareBehaviors { get; set; }
}
