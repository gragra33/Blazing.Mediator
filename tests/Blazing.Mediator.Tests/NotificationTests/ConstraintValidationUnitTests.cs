using Blazing.Mediator.Configuration;

namespace Blazing.Mediator.Tests.NotificationTests;

/// <summary>
/// Simple unit tests for Phase 2 constraint validation functionality
/// </summary>
public class ConstraintValidationUnitTests
{
    [Fact]
    public void ConstraintValidationOptions_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new ConstraintValidationOptions();

        // Assert
        options.Strictness.ShouldBe(ConstraintValidationOptions.ValidationStrictness.Lenient);
        options.EnableConstraintCaching.ShouldBeTrue();
        options.FailFastOnErrors.ShouldBeFalse();
        options.ValidateNestedGenericConstraints.ShouldBeTrue();
        options.MaxConstraintInheritanceDepth.ShouldBe(10);
        options.ValidateCircularDependencies.ShouldBeTrue();
        options.EnableDetailedLogging.ShouldBeFalse();
    }

    [Fact]
    public void ConstraintValidationOptions_CreateStrict_HasCorrectSettings()
    {
        // Act
        var options = ConstraintValidationOptions.CreateStrict();

        // Assert
        options.Strictness.ShouldBe(ConstraintValidationOptions.ValidationStrictness.Strict);
        options.FailFastOnErrors.ShouldBeTrue();
        options.ValidateNestedGenericConstraints.ShouldBeTrue();
        options.ValidateCircularDependencies.ShouldBeTrue();
    }

    [Fact]
    public void ConstraintValidationOptions_CreateLenient_HasCorrectSettings()
    {
        // Act
        var options = ConstraintValidationOptions.CreateLenient();

        // Assert
        options.Strictness.ShouldBe(ConstraintValidationOptions.ValidationStrictness.Lenient);
        options.FailFastOnErrors.ShouldBeFalse();
        options.ValidateNestedGenericConstraints.ShouldBeTrue();
    }

    [Fact]
    public void ConstraintValidationOptions_CreateDisabled_HasCorrectSettings()
    {
        // Act
        var options = ConstraintValidationOptions.CreateDisabled();

        // Assert
        options.Strictness.ShouldBe(ConstraintValidationOptions.ValidationStrictness.Disabled);
        options.EnableConstraintCaching.ShouldBeFalse();
        options.ValidateNestedGenericConstraints.ShouldBeFalse();
        options.ValidateCircularDependencies.ShouldBeFalse();
    }

    [Fact]
    public void ConstraintValidationOptions_Validate_DetectsInvalidSettings()
    {
        // Arrange
        var options = new ConstraintValidationOptions
        {
            MaxConstraintInheritanceDepth = 0 // Invalid
        };

        // Act
        var errors = options.Validate();

        // Assert
        errors.ShouldNotBeEmpty();
        errors.ShouldContain(e => e.Contains("MaxConstraintInheritanceDepth"));
    }

    [Fact]
    public void ConstraintValidationOptions_ValidateAndThrow_ThrowsOnErrors()
    {
        // Arrange
        var options = new ConstraintValidationOptions
        {
            MaxConstraintInheritanceDepth = -1
        };

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => options.ValidateAndThrow())
            .Message.ShouldContain("Constraint validation configuration is invalid");
    }

    [Fact]
    public void ConstraintValidationOptions_Clone_CreatesIndependentCopy()
    {
        // Arrange
        var original = ConstraintValidationOptions.CreateStrict();
        original.MaxConstraintInheritanceDepth = 15;

        // Act
        var clone = original.Clone();

        // Assert
        clone.ShouldNotBe(original);
        clone.Strictness.ShouldBe(original.Strictness);
        clone.MaxConstraintInheritanceDepth.ShouldBe(15);

        // Modify original to ensure independence
        original.MaxConstraintInheritanceDepth = 20;
        clone.MaxConstraintInheritanceDepth.ShouldBe(15); // Should remain unchanged
    }

    [Fact]
    public void ConstraintValidationOptions_CustomValidationRules_WorkCorrectly()
    {
        // Arrange
        var options = new ConstraintValidationOptions();
        options.CustomValidationRules[typeof(INotification)] = (middleware, notification) => true;

        // Act
        var errors = options.Validate();

        // Assert
        errors.ShouldBeEmpty(); // No errors with valid custom rules
    }

    [Fact]
    public void ConstraintValidationOptions_ExcludedTypes_WorkCorrectly()
    {
        // Arrange
        var options = new ConstraintValidationOptions();
        options.ExcludedTypes.Add(typeof(string));

        // Act
        var errors = options.Validate();

        // Assert
        errors.ShouldBeEmpty(); // No errors with valid excluded types
    }
}