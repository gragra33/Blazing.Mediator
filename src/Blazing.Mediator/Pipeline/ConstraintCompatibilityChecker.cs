using Blazing.Mediator.Configuration;
using System.Collections.Concurrent;
using System.Reflection;

namespace Blazing.Mediator.Pipeline;

/// <summary>
/// Advanced constraint compatibility checker for type-constrained notification middleware.
/// Provides comprehensive validation of generic constraints with performance optimization.
/// </summary>
public sealed class ConstraintCompatibilityChecker
{
    private readonly ConstraintValidationOptions _options;
    private readonly ILogger<ConstraintCompatibilityChecker>? _logger;
    
    // Cache for constraint compatibility results to improve performance
    private readonly ConcurrentDictionary<(Type MiddlewareType, Type NotificationType), bool> _compatibilityCache = new();
    
    // Cache for constraint analysis results
    private readonly ConcurrentDictionary<Type, ConstraintAnalysisResult> _constraintAnalysisCache = new();

    public ConstraintCompatibilityChecker(ConstraintValidationOptions? options = null, ILogger<ConstraintCompatibilityChecker>? logger = null)
    {
        _options = options ?? ConstraintValidationOptions.CreateLenient();
        _logger = logger;
    }

    /// <summary>
    /// Checks if a middleware type is compatible with a notification type based on constraints.
    /// </summary>
    /// <param name="middlewareType">The middleware type to check.</param>
    /// <param name="notificationType">The notification type to check compatibility with.</param>
    /// <returns>True if compatible, false otherwise.</returns>
    public bool IsCompatible(Type middlewareType, Type notificationType)
    {
        if (_options.Strictness == ConstraintValidationOptions.ValidationStrictness.Disabled)
        {
            return true; // Skip validation entirely
        }

        // Check if types are excluded from validation
        if (_options.ExcludedTypes.Contains(middlewareType) || _options.ExcludedTypes.Contains(notificationType))
        {
            return true;
        }

        // Check cache first for performance
        if (_options.EnableConstraintCaching)
        {
            var cacheKey = (middlewareType, notificationType);
            if (_compatibilityCache.TryGetValue(cacheKey, out bool cachedResult))
            {
                LogDetailedMessage($"Cache hit for {middlewareType.Name} with {notificationType.Name}: {cachedResult}");
                return cachedResult;
            }
        }

        try
        {
            bool isCompatible = CheckCompatibilityInternal(middlewareType, notificationType);

            // Cache the result if caching is enabled
            if (_options.EnableConstraintCaching)
            {
                var cacheKey = (middlewareType, notificationType);
                _compatibilityCache.TryAdd(cacheKey, isCompatible);
            }

            return isCompatible;
        }
        catch (Exception ex) when (_options.Strictness != ConstraintValidationOptions.ValidationStrictness.Strict)
        {
            // In lenient mode, log the error but return true to allow execution
            _logger?.LogWarning(ex, "Error checking constraint compatibility between {MiddlewareType} and {NotificationType}, allowing execution in lenient mode",
                middlewareType.Name, notificationType.Name);
            return true;
        }
    }

    /// <summary>
    /// Performs detailed constraint analysis for a middleware type.
    /// </summary>
    /// <param name="middlewareType">The middleware type to analyze.</param>
    /// <returns>Detailed constraint analysis result.</returns>
    public ConstraintAnalysisResult AnalyzeConstraints(Type middlewareType)
    {
        if (_options.EnableConstraintCaching && _constraintAnalysisCache.TryGetValue(middlewareType, out var cachedAnalysis))
        {
            return cachedAnalysis;
        }

        var analysis = PerformConstraintAnalysis(middlewareType);

        if (_options.EnableConstraintCaching)
        {
            _constraintAnalysisCache.TryAdd(middlewareType, analysis);
        }

        return analysis;
    }

    /// <summary>
    /// Validates constraint compatibility and returns detailed validation results.
    /// </summary>
    /// <param name="middlewareType">The middleware type to validate.</param>
    /// <param name="notificationType">The notification type to validate against.</param>
    /// <returns>Validation result with details about compatibility.</returns>
    public ConstraintValidationResult ValidateConstraints(Type middlewareType, Type notificationType)
    {
        var result = new ConstraintValidationResult
        {
            MiddlewareType = middlewareType,
            NotificationType = notificationType,
            IsValid = true,
            ValidationErrors = new List<string>(),
            ValidationWarnings = new List<string>()
        };

        if (_options.Strictness == ConstraintValidationOptions.ValidationStrictness.Disabled)
        {
            result.SkippedValidation = true;
            return result;
        }

        try
        {
            // Check basic INotification compatibility
            if (!typeof(INotification).IsAssignableFrom(notificationType))
            {
                result.IsValid = false;
                result.ValidationErrors.Add($"Notification type {notificationType.Name} does not implement INotification");
                
                if (_options.FailFastOnErrors)
                {
                    return result;
                }
            }

            // Analyze middleware constraints
            var analysis = AnalyzeConstraints(middlewareType);
            
            if (analysis.HasConstraints)
            {
                result.IsValid = ValidateConstraintsAgainstNotification(analysis, notificationType, result);
            }
            else
            {
                // No constraints means it's compatible with all notifications
                result.ValidationWarnings.Add($"Middleware {middlewareType.Name} has no type constraints and will execute for all notifications");
            }

            // Check for custom validation rules
            foreach (var customRule in _options.CustomValidationRules)
            {
                if (customRule.Key.IsAssignableFrom(notificationType))
                {
                    bool customResult = customRule.Value(middlewareType, notificationType);
                    if (!customResult)
                    {
                        result.IsValid = false;
                        result.ValidationErrors.Add($"Custom validation rule failed for constraint {customRule.Key.Name}");
                        
                        if (_options.FailFastOnErrors)
                        {
                            return result;
                        }
                    }
                }
            }

            LogDetailedMessage($"Constraint validation for {middlewareType.Name} with {notificationType.Name}: Valid={result.IsValid}, Errors={result.ValidationErrors.Count}, Warnings={result.ValidationWarnings.Count}");

        }
        catch (Exception ex)
        {
            result.IsValid = _options.Strictness != ConstraintValidationOptions.ValidationStrictness.Strict;
            result.ValidationErrors.Add($"Exception during constraint validation: {ex.Message}");
            
            if (_options.Strictness == ConstraintValidationOptions.ValidationStrictness.Strict)
            {
                throw;
            }

            _logger?.LogWarning(ex, "Exception during constraint validation for {MiddlewareType} and {NotificationType}",
                middlewareType.Name, notificationType.Name);
        }

        return result;
    }

    /// <summary>
    /// Clears all cached constraint compatibility results.
    /// </summary>
    public void ClearCache()
    {
        _compatibilityCache.Clear();
        _constraintAnalysisCache.Clear();
        LogDetailedMessage("Constraint compatibility cache cleared");
    }

    private bool CheckCompatibilityInternal(Type middlewareType, Type notificationType)
    {
        LogDetailedMessage($"Checking compatibility between {middlewareType.Name} and {notificationType.Name}");

        // Check if middleware implements INotificationMiddleware<T>
        var constrainedInterfaces = GetConstrainedMiddlewareInterfaces(middlewareType);
        
        if (!constrainedInterfaces.Any())
        {
            // No constraints - compatible with all notifications
            LogDetailedMessage($"Middleware {middlewareType.Name} has no constraints - compatible with all notifications");
            return true;
        }

        // Check if any constraint is compatible with the notification type
        foreach (var constrainedInterface in constrainedInterfaces)
        {
            var constraintType = constrainedInterface.GetGenericArguments()[0];
            
            if (IsConstraintCompatible(constraintType, notificationType))
            {
                LogDetailedMessage($"Compatible constraint found: {constraintType.Name} is compatible with {notificationType.Name}");
                return true;
            }
        }

        LogDetailedMessage($"No compatible constraints found for {middlewareType.Name} with {notificationType.Name}");
        return false;
    }

    private bool IsConstraintCompatible(Type constraintType, Type notificationType)
    {
        // Basic assignability check
        if (constraintType.IsAssignableFrom(notificationType))
        {
            return true;
        }

        // Enhanced generic constraint checking
        if (_options.ValidateNestedGenericConstraints)
        {
            return CheckNestedGenericConstraints(constraintType, notificationType, 0);
        }

        return false;
    }

    private bool CheckNestedGenericConstraints(Type constraintType, Type notificationType, int depth)
    {
        if (depth > _options.MaxConstraintInheritanceDepth)
        {
            LogDetailedMessage($"Maximum constraint inheritance depth {_options.MaxConstraintInheritanceDepth} exceeded");
            return false;
        }

        // Check direct assignability first
        if (constraintType.IsAssignableFrom(notificationType))
        {
            return true;
        }

        // Check implemented interfaces
        var interfaces = notificationType.GetInterfaces();
        foreach (var iface in interfaces)
        {
            if (constraintType.IsAssignableFrom(iface))
            {
                return true;
            }

            // Recursively check interface inheritance
            if (iface.IsGenericType && CheckNestedGenericConstraints(constraintType, iface, depth + 1))
            {
                return true;
            }
        }

        // Check base class hierarchy
        var baseType = notificationType.BaseType;
        if (baseType != null && baseType != typeof(object))
        {
            if (CheckNestedGenericConstraints(constraintType, baseType, depth + 1))
            {
                return true;
            }
        }

        return false;
    }

    private ConstraintAnalysisResult PerformConstraintAnalysis(Type middlewareType)
    {
        var result = new ConstraintAnalysisResult
        {
            MiddlewareType = middlewareType,
            ConstraintTypes = new List<Type>(),
            HasConstraints = false,
            IsGeneric = middlewareType.IsGenericType || middlewareType.IsGenericTypeDefinition,
            ValidationWarnings = new List<string>()
        };

        var constrainedInterfaces = GetConstrainedMiddlewareInterfaces(middlewareType);
        
        if (constrainedInterfaces.Any())
        {
            result.HasConstraints = true;
            result.ConstraintTypes = constrainedInterfaces
                .Select(i => i.GetGenericArguments()[0])
                .Distinct()
                .ToList();

            // Check for potential issues
            if (result.ConstraintTypes.Count > 1)
            {
                result.ValidationWarnings.Add($"Middleware {middlewareType.Name} has multiple constraint types, which may limit its applicability");
            }

            // Validate circular dependencies if enabled
            if (_options.ValidateCircularDependencies)
            {
                foreach (var constraintType in result.ConstraintTypes)
                {
                    if (HasCircularDependency(constraintType, new HashSet<Type>()))
                    {
                        result.ValidationWarnings.Add($"Potential circular dependency detected in constraint type {constraintType.Name}");
                    }
                }
            }
        }

        return result;
    }

    private bool ValidateConstraintsAgainstNotification(ConstraintAnalysisResult analysis, Type notificationType, ConstraintValidationResult result)
    {
        bool isValid = true;

        foreach (var constraintType in analysis.ConstraintTypes)
        {
            if (!IsConstraintCompatible(constraintType, notificationType))
            {
                isValid = false;
                result.ValidationErrors.Add($"Notification type {notificationType.Name} is not compatible with constraint {constraintType.Name}");
                
                if (_options.FailFastOnErrors)
                {
                    break;
                }
            }
            else
            {
                LogDetailedMessage($"Constraint {constraintType.Name} is compatible with notification {notificationType.Name}");
            }
        }

        return isValid;
    }

    private bool HasCircularDependency(Type type, HashSet<Type> visited)
    {
        if (visited.Contains(type))
        {
            return true; // Circular dependency detected
        }

        visited.Add(type);

        try
        {
            // Check interfaces for circular dependencies
            foreach (var iface in type.GetInterfaces())
            {
                if (HasCircularDependency(iface, new HashSet<Type>(visited)))
                {
                    return true;
                }
            }

            // Check base type for circular dependencies
            if (type.BaseType != null && type.BaseType != typeof(object))
            {
                if (HasCircularDependency(type.BaseType, new HashSet<Type>(visited)))
                {
                    return true;
                }
            }
        }
        finally
        {
            visited.Remove(type);
        }

        return false;
    }

    private static Type[] GetConstrainedMiddlewareInterfaces(Type middlewareType)
    {
        return middlewareType.GetInterfaces()
            .Where(i => i.IsGenericType && 
                       i.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>))
            .ToArray();
    }

    private void LogDetailedMessage(string message)
    {
        if (_options.EnableDetailedLogging)
        {
            _logger?.LogDebug("ConstraintChecker: {Message}", message);
        }
    }
}

/// <summary>
/// Result of constraint analysis for a middleware type.
/// </summary>
public sealed class ConstraintAnalysisResult
{
    public Type MiddlewareType { get; set; } = null!;
    public List<Type> ConstraintTypes { get; set; } = new();
    public bool HasConstraints { get; set; }
    public bool IsGeneric { get; set; }
    public List<string> ValidationWarnings { get; set; } = new();
}

/// <summary>
/// Result of constraint validation between a middleware type and notification type.
/// </summary>
public sealed class ConstraintValidationResult
{
    public Type MiddlewareType { get; set; } = null!;
    public Type NotificationType { get; set; } = null!;
    public bool IsValid { get; set; }
    public bool SkippedValidation { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public List<string> ValidationWarnings { get; set; } = new();
}