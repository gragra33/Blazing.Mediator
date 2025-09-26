using System.ComponentModel.DataAnnotations;

namespace Blazing.Mediator.Configuration;

/// <summary>
/// Configuration options for type constraint validation in notification middleware.
/// Controls how strictly constraint compatibility is enforced during registration and execution.
/// </summary>
public sealed class ConstraintValidationOptions
{
    /// <summary>
    /// Defines the strictness level for constraint validation.
    /// </summary>
    public enum ValidationStrictness
    {
        /// <summary>
        /// Strict validation - fails immediately on any constraint compatibility issues.
        /// Provides compile-time safety but may prevent some valid scenarios.
        /// </summary>
        Strict = 0,

        /// <summary>
        /// Lenient validation - logs warnings for potential issues but allows registration.
        /// Provides runtime flexibility while still alerting to potential problems.
        /// </summary>
        Lenient = 1,

        /// <summary>
        /// Disabled validation - skips constraint validation entirely.
        /// Provides maximum runtime flexibility but no constraint safety checks.
        /// </summary>
        Disabled = 2
    }

    /// <summary>
    /// Gets or sets the validation strictness level.
    /// Default is Lenient to balance safety with flexibility.
    /// </summary>
    public ValidationStrictness Strictness { get; set; } = ValidationStrictness.Lenient;

    /// <summary>
    /// Gets or sets whether to enable constraint resolution caching.
    /// When enabled, constraint compatibility results are cached for better performance.
    /// Default is true for performance optimization.
    /// </summary>
    public bool EnableConstraintCaching { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to fail fast on the first constraint validation error.
    /// When true, stops validation on first error. When false, collects all errors.
    /// Default is false to provide comprehensive error reporting.
    /// </summary>
    public bool FailFastOnErrors { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to validate nested generic constraints.
    /// When true, validates complex generic type hierarchies more thoroughly.
    /// Default is true for comprehensive constraint checking.
    /// </summary>
    public bool ValidateNestedGenericConstraints { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to allow runtime constraint resolution changes.
    /// When true, constraint validation can be modified at runtime.
    /// Default is false for consistent behavior.
    /// </summary>
    public bool AllowRuntimeConfigurationChanges { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum depth for constraint inheritance chain validation.
    /// Prevents infinite loops in complex inheritance hierarchies.
    /// Default is 10, which handles most real-world scenarios.
    /// </summary>
    [Range(1, 100)]
    public int MaxConstraintInheritanceDepth { get; set; } = 10;

    /// <summary>
    /// Gets or sets whether to validate circular dependencies in constraint interfaces.
    /// When true, checks for circular dependencies that could cause infinite loops.
    /// Default is true for safety.
    /// </summary>
    public bool ValidateCircularDependencies { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to log constraint validation details.
    /// When true, provides detailed logging for constraint resolution process.
    /// Default is false to avoid log noise in production.
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;

    /// <summary>
    /// Gets or sets custom constraint validation rules.
    /// Allows for application-specific constraint validation logic.
    /// </summary>
    public Dictionary<Type, Func<Type, Type, bool>> CustomValidationRules { get; set; } = new();

    /// <summary>
    /// Gets or sets types to exclude from constraint validation.
    /// Useful for excluding problematic types from constraint checking.
    /// </summary>
    public HashSet<Type> ExcludedTypes { get; set; } = new();

    /// <summary>
    /// Creates a copy of the current options with the same configuration.
    /// </summary>
    /// <returns>A new instance with identical configuration.</returns>
    public ConstraintValidationOptions Clone()
    {
        return new ConstraintValidationOptions
        {
            Strictness = Strictness,
            EnableConstraintCaching = EnableConstraintCaching,
            FailFastOnErrors = FailFastOnErrors,
            ValidateNestedGenericConstraints = ValidateNestedGenericConstraints,
            AllowRuntimeConfigurationChanges = AllowRuntimeConfigurationChanges,
            MaxConstraintInheritanceDepth = MaxConstraintInheritanceDepth,
            ValidateCircularDependencies = ValidateCircularDependencies,
            EnableDetailedLogging = EnableDetailedLogging,
            CustomValidationRules = new Dictionary<Type, Func<Type, Type, bool>>(CustomValidationRules),
            ExcludedTypes = new HashSet<Type>(ExcludedTypes)
        };
    }

    /// <summary>
    /// Validates the configuration and returns any validation errors.
    /// </summary>
    /// <returns>List of validation error messages.</returns>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (!Enum.IsDefined(typeof(ValidationStrictness), Strictness))
        {
            errors.Add($"Invalid validation strictness value: {Strictness}");
        }

        if (MaxConstraintInheritanceDepth < 1 || MaxConstraintInheritanceDepth > 100)
        {
            errors.Add($"MaxConstraintInheritanceDepth must be between 1 and 100, got: {MaxConstraintInheritanceDepth}");
        }

        // Validate custom validation rules
        foreach (var kvp in CustomValidationRules)
        {
            if (kvp.Key == null)
            {
                errors.Add("Custom validation rules cannot have null constraint types");
                continue;
            }

            if (kvp.Value == null)
            {
                errors.Add($"Custom validation rule for type {kvp.Key.Name} cannot have null validation function");
                continue;
            }

            // Validate that the constraint type is an interface or class that implements INotification
            if (!kvp.Key.IsInterface && !kvp.Key.IsClass)
            {
                errors.Add($"Custom validation rule constraint type {kvp.Key.Name} must be an interface or class");
                continue;
            }

            if (!typeof(INotification).IsAssignableFrom(kvp.Key))
            {
                errors.Add($"Custom validation rule constraint type {kvp.Key.Name} must implement INotification");
            }
        }

        // Validate excluded types
        foreach (var excludedType in ExcludedTypes)
        {
            if (excludedType == null)
            {
                errors.Add("ExcludedTypes cannot contain null types");
                continue;
            }

            if (excludedType == typeof(INotification))
            {
                errors.Add("Cannot exclude INotification from constraint validation");
            }
        }

        return errors;
    }

    /// <summary>
    /// Validates the configuration and throws an exception if there are errors.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
    public void ValidateAndThrow()
    {
        var errors = Validate();
        if (errors.Count > 0)
        {
            throw new InvalidOperationException($"Constraint validation configuration is invalid: {string.Join("; ", errors)}");
        }
    }

    /// <summary>
    /// Creates default strict validation options.
    /// </summary>
    /// <returns>New instance configured for strict validation.</returns>
    public static ConstraintValidationOptions CreateStrict()
    {
        return new ConstraintValidationOptions
        {
            Strictness = ValidationStrictness.Strict,
            FailFastOnErrors = true,
            ValidateNestedGenericConstraints = true,
            ValidateCircularDependencies = true,
            EnableDetailedLogging = false
        };
    }

    /// <summary>
    /// Creates default lenient validation options.
    /// </summary>
    /// <returns>New instance configured for lenient validation.</returns>
    public static ConstraintValidationOptions CreateLenient()
    {
        return new ConstraintValidationOptions
        {
            Strictness = ValidationStrictness.Lenient,
            FailFastOnErrors = false,
            ValidateNestedGenericConstraints = true,
            ValidateCircularDependencies = true,
            EnableDetailedLogging = false
        };
    }

    /// <summary>
    /// Creates validation options with all validation disabled.
    /// </summary>
    /// <returns>New instance configured to disable all validation.</returns>
    public static ConstraintValidationOptions CreateDisabled()
    {
        return new ConstraintValidationOptions
        {
            Strictness = ValidationStrictness.Disabled,
            EnableConstraintCaching = false,
            ValidateNestedGenericConstraints = false,
            ValidateCircularDependencies = false,
            EnableDetailedLogging = false
        };
    }
}