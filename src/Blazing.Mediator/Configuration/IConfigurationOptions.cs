namespace Blazing.Mediator.Configuration;

/// <summary>
/// Defines the common contract for mediator configuration options classes.
/// Provides validation, cloning, and static factory method contracts for consistent configuration patterns.
/// </summary>
/// <typeparam name="T">The concrete configuration options type</typeparam>
public interface IConfigurationOptions<T> where T : class, IConfigurationOptions<T>
{
    /// <summary>
    /// Validates the current configuration and returns any validation errors.
    /// </summary>
    /// <returns>A list of validation error messages, or an empty list if valid.</returns>
    IReadOnlyList<string> Validate();

    /// <summary>
    /// Validates the configuration and throws an exception if invalid.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the configuration is invalid.</exception>
    void ValidateAndThrow();

    /// <summary>
    /// Creates a copy of the current options with all the same values.
    /// </summary>
    /// <returns>A new instance of the configuration options with the same configuration.</returns>
    T Clone();
}

/// <summary>
/// Defines the contract for configuration options classes that support environment-specific factory methods.
/// Extends IConfigurationOptions with standard factory methods for different deployment scenarios.
/// </summary>
/// <typeparam name="T">The concrete configuration options type</typeparam>
public interface IEnvironmentConfigurationOptions<T> : IConfigurationOptions<T> 
    where T : class, IEnvironmentConfigurationOptions<T>
{
    /// <summary>
    /// Creates a configuration suitable for development environments.
    /// Typically enables comprehensive features with detailed information for debugging.
    /// </summary>
    /// <returns>A new instance configured for development scenarios.</returns>
    static abstract T Development();

    /// <summary>
    /// Creates a configuration suitable for production environments.
    /// Typically enables essential features with optimized performance settings.
    /// </summary>
    /// <returns>A new instance configured for production scenarios.</returns>
    static abstract T Production();

    /// <summary>
    /// Creates a configuration with all features disabled.
    /// Useful for high-performance scenarios where the feature is not needed.
    /// </summary>
    /// <returns>A new instance with all features disabled.</returns>
    static abstract T Disabled();
}