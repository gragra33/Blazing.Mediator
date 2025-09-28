using Blazing.Mediator.Configuration;

namespace Blazing.Mediator.Tests.ConfigurationTests;

/// <summary>
/// Comprehensive tests for MediatorTelemetryOptions configuration and behavior.
/// Validates all telemetry features including packet-level telemetry, performance thresholds,
/// middleware details capture, and streaming telemetry options.
/// </summary>
public class MediatorTelemetryOptionsTests
{
    #region Constructor and Default Values Tests

    [Fact]
    public void DefaultConstructor_ShouldSetCorrectDefaultValues()
    {
        // Act
        var options = new MediatorTelemetryOptions();

        // Assert
        options.Enabled.ShouldBeTrue();
        options.CaptureMiddlewareDetails.ShouldBeTrue();
        options.CaptureHandlerDetails.ShouldBeTrue();
        options.CaptureExceptionDetails.ShouldBeTrue();
        options.EnableHealthChecks.ShouldBeTrue();
        options.MaxExceptionMessageLength.ShouldBe(200);
        options.MaxStackTraceLines.ShouldBe(3);
        options.PacketLevelTelemetryEnabled.ShouldBeFalse();
        options.PacketTelemetryBatchSize.ShouldBe(10);
        options.EnableStreamingMetrics.ShouldBeTrue();
        options.CapturePacketSize.ShouldBeFalse();
        options.EnableStreamingPerformanceClassification.ShouldBeTrue();
        options.ExcellentPerformanceThreshold.ShouldBe(0.1);
        options.GoodPerformanceThreshold.ShouldBe(0.3);
        options.FairPerformanceThreshold.ShouldBe(0.5);
        options.SensitiveDataPatterns.ShouldNotBeEmpty();
    }

    [Fact]
    public void DefaultConstructor_ShouldSetCorrectSensitiveDataPatterns()
    {
        // Act
        var options = new MediatorTelemetryOptions();

        // Assert
        var expectedPatterns = new[] { "password", "token", "secret", "key", "auth", "credential", "connection" };
        options.SensitiveDataPatterns.Count.ShouldBe(expectedPatterns.Length);
        
        foreach (var pattern in expectedPatterns)
        {
            options.SensitiveDataPatterns.ShouldContain(pattern);
        }
    }

    #endregion

    #region Basic Property Tests

    [Fact]
    public void Enabled_WhenSetToFalse_ShouldDisableTelemetry()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.Enabled = false;

        // Assert
        options.Enabled.ShouldBeFalse();
    }

    [Fact]
    public void CaptureMiddlewareDetails_WhenSetToFalse_ShouldDisableMiddlewareCapture()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.CaptureMiddlewareDetails = false;

        // Assert
        options.CaptureMiddlewareDetails.ShouldBeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CaptureHandlerDetails_WhenSetToValue_ShouldUpdateCorrectly(bool value)
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.CaptureHandlerDetails = value;

        // Assert
        options.CaptureHandlerDetails.ShouldBe(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CaptureExceptionDetails_WhenSetToValue_ShouldUpdateCorrectly(bool value)
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.CaptureExceptionDetails = value;

        // Assert
        options.CaptureExceptionDetails.ShouldBe(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnableHealthChecks_WhenSetToValue_ShouldUpdateCorrectly(bool value)
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.EnableHealthChecks = value;

        // Assert
        options.EnableHealthChecks.ShouldBe(value);
    }

    #endregion

    #region Numeric Value Tests

    [Fact]
    public void MaxExceptionMessageLength_WhenSetToValidValue_ShouldUpdateCorrectly()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.MaxExceptionMessageLength = 500;

        // Assert
        options.MaxExceptionMessageLength.ShouldBe(500);
    }

    [Theory]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]
    public void MaxExceptionMessageLength_WhenSetToValidValues_ShouldUpdateCorrectly(int value)
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.MaxExceptionMessageLength = value;

        // Assert
        options.MaxExceptionMessageLength.ShouldBe(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void MaxStackTraceLines_WhenSetToValidValue_ShouldUpdateCorrectly(int value)
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.MaxStackTraceLines = value;

        // Assert
        options.MaxStackTraceLines.ShouldBe(value);
    }

    [Fact]
    public void PacketTelemetryBatchSize_WhenSetToValidValue_ShouldUpdateCorrectly()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.PacketTelemetryBatchSize = 50;

        // Assert
        options.PacketTelemetryBatchSize.ShouldBe(50);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(50)]
    [InlineData(100)]
    public void PacketTelemetryBatchSize_WhenSetToValidValues_ShouldUpdateCorrectly(int value)
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.PacketTelemetryBatchSize = value;

        // Assert
        options.PacketTelemetryBatchSize.ShouldBe(value);
    }

    #endregion

    #region Streaming Configuration Tests

    [Fact]
    public void PacketLevelTelemetryEnabled_WhenSetToTrue_ShouldEnablePacketTelemetry()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.PacketLevelTelemetryEnabled = true;

        // Assert
        options.PacketLevelTelemetryEnabled.ShouldBeTrue();
    }

    [Fact]
    public void EnableStreamingMetrics_WhenSetToFalse_ShouldDisableStreamingMetrics()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.EnableStreamingMetrics = false;

        // Assert
        options.EnableStreamingMetrics.ShouldBeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnableStreamingMetrics_WhenSetToValue_ShouldUpdateCorrectly(bool value)
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.EnableStreamingMetrics = value;

        // Assert
        options.EnableStreamingMetrics.ShouldBe(value);
    }

    [Fact]
    public void CapturePacketSize_WhenSetToTrue_ShouldEnablePacketSizeCapture()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.CapturePacketSize = true;

        // Assert
        options.CapturePacketSize.ShouldBeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CapturePacketSize_WhenSetToValue_ShouldUpdateCorrectly(bool value)
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.CapturePacketSize = value;

        // Assert
        options.CapturePacketSize.ShouldBe(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnableStreamingPerformanceClassification_WhenSetToValue_ShouldUpdateCorrectly(bool value)
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.EnableStreamingPerformanceClassification = value;

        // Assert
        options.EnableStreamingPerformanceClassification.ShouldBe(value);
    }

    #endregion

    #region Performance Threshold Tests

    [Fact]
    public void PerformanceThresholds_WhenSetToCustomValues_ShouldUpdateCorrectly()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.ExcellentPerformanceThreshold = 0.05;
        options.GoodPerformanceThreshold = 0.2;
        options.FairPerformanceThreshold = 0.4;

        // Assert
        options.ExcellentPerformanceThreshold.ShouldBe(0.05);
        options.GoodPerformanceThreshold.ShouldBe(0.2);
        options.FairPerformanceThreshold.ShouldBe(0.4);
    }

    [Theory]
    [InlineData(0.05)]
    [InlineData(0.1)]
    [InlineData(0.15)]
    [InlineData(0.2)]
    public void ExcellentPerformanceThreshold_WhenSetToValidValue_ShouldUpdateCorrectly(double value)
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.ExcellentPerformanceThreshold = value;

        // Assert
        options.ExcellentPerformanceThreshold.ShouldBe(value);
    }

    [Theory]
    [InlineData(0.2)]
    [InlineData(0.3)]
    [InlineData(0.4)]
    [InlineData(0.5)]
    public void GoodPerformanceThreshold_WhenSetToValidValue_ShouldUpdateCorrectly(double value)
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.GoodPerformanceThreshold = value;

        // Assert
        options.GoodPerformanceThreshold.ShouldBe(value);
    }

    [Theory]
    [InlineData(0.4)]
    [InlineData(0.5)]
    [InlineData(0.6)]
    [InlineData(0.7)]
    public void FairPerformanceThreshold_WhenSetToValidValue_ShouldUpdateCorrectly(double value)
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.FairPerformanceThreshold = value;

        // Assert
        options.FairPerformanceThreshold.ShouldBe(value);
    }

    [Fact]
    public void PerformanceThresholds_WhenSetToCustomValues_ShouldUpdateAllCorrectly()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.ExcellentPerformanceThreshold = 0.05;
        options.GoodPerformanceThreshold = 0.2;
        options.FairPerformanceThreshold = 0.4;

        // Assert
        options.ExcellentPerformanceThreshold.ShouldBe(0.05);
        options.GoodPerformanceThreshold.ShouldBe(0.2);
        options.FairPerformanceThreshold.ShouldBe(0.4);
    }

    #endregion

    #region Sensitive Data Patterns Tests

    [Fact]
    public void SensitiveDataPatterns_WhenModified_ShouldAllowCustomization()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.SensitiveDataPatterns.Add("custom-pattern");

        // Assert
        options.SensitiveDataPatterns.ShouldContain("custom-pattern");
        options.SensitiveDataPatterns.ShouldContain("password");
    }

    [Fact]
    public void SensitiveDataPatterns_WhenModified_ShouldAllowMultipleCustomizations()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.SensitiveDataPatterns.Add("custom-pattern");
        options.SensitiveDataPatterns.Add("api-key");

        // Assert
        options.SensitiveDataPatterns.ShouldContain("custom-pattern");
        options.SensitiveDataPatterns.ShouldContain("api-key");
        options.SensitiveDataPatterns.ShouldContain("password"); // Original pattern should still exist
    }

    [Fact]
    public void SensitiveDataPatterns_WhenCleared_ShouldRemoveAllPatterns()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.SensitiveDataPatterns.Clear();

        // Assert
        options.SensitiveDataPatterns.ShouldBeEmpty();
    }

    [Fact]
    public void SensitiveDataPatterns_WhenReplacedWithCustomList_ShouldUseCustomList()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();
        var customPatterns = new List<string> { "custom1", "custom2", "custom3" };

        // Act
        options.SensitiveDataPatterns = customPatterns;

        // Assert
        options.SensitiveDataPatterns.Count.ShouldBe(3);
        options.SensitiveDataPatterns.ShouldContain("custom1");
        options.SensitiveDataPatterns.ShouldContain("custom2");
        options.SensitiveDataPatterns.ShouldContain("custom3");
        options.SensitiveDataPatterns.ShouldNotContain("password"); // Original pattern should be gone
    }

    [Fact]
    public void SensitiveDataPatterns_WhenRemoving_ShouldRemoveSpecificPattern()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();
        var originalCount = options.SensitiveDataPatterns.Count;

        // Act
        options.SensitiveDataPatterns.Remove("password");

        // Assert
        options.SensitiveDataPatterns.Count.ShouldBe(originalCount - 1);
        options.SensitiveDataPatterns.ShouldNotContain("password");
        options.SensitiveDataPatterns.ShouldContain("token"); // Other patterns should remain
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void MaxExceptionMessageLength_WhenSetToZeroOrNegative_ShouldAcceptValue(int value)
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act & Assert - Should not throw, behavior is up to consuming code
        options.MaxExceptionMessageLength = value;
        options.MaxExceptionMessageLength.ShouldBe(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void MaxStackTraceLines_WhenSetToZeroOrNegative_ShouldAcceptValue(int value)
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act & Assert - Should not throw, behavior is up to consuming code
        options.MaxStackTraceLines = value;
        options.MaxStackTraceLines.ShouldBe(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void PacketTelemetryBatchSize_WhenSetToZeroOrNegative_ShouldAcceptValue(int value)
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act & Assert - Should not throw, behavior is up to consuming code
        options.PacketTelemetryBatchSize = value;
        options.PacketTelemetryBatchSize.ShouldBe(value);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-0.1)]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public void PerformanceThresholds_WhenSetToEdgeValues_ShouldAcceptValues(double value)
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act & Assert - Should not throw, behavior is up to consuming code
        options.ExcellentPerformanceThreshold = value;
        options.GoodPerformanceThreshold = value;
        options.FairPerformanceThreshold = value;

        options.ExcellentPerformanceThreshold.ShouldBe(value);
        options.GoodPerformanceThreshold.ShouldBe(value);
        options.FairPerformanceThreshold.ShouldBe(value);
    }

    [Theory]
    [InlineData(double.MaxValue)]
    [InlineData(double.MinValue)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void PerformanceThresholds_WhenSetToExtremeValues_ShouldAcceptValues(double value)
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act & Assert - Should not throw, behavior is up to consuming code
        options.ExcellentPerformanceThreshold = value;
        options.GoodPerformanceThreshold = value;
        options.FairPerformanceThreshold = value;

        options.ExcellentPerformanceThreshold.ShouldBe(value);
        options.GoodPerformanceThreshold.ShouldBe(value);
        options.FairPerformanceThreshold.ShouldBe(value);
    }

    [Fact]
    public void PerformanceThresholds_WhenSetToNaN_ShouldAcceptNaN()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.ExcellentPerformanceThreshold = double.NaN;
        options.GoodPerformanceThreshold = double.NaN;
        options.FairPerformanceThreshold = double.NaN;

        // Assert
        double.IsNaN(options.ExcellentPerformanceThreshold).ShouldBeTrue();
        double.IsNaN(options.GoodPerformanceThreshold).ShouldBeTrue();
        double.IsNaN(options.FairPerformanceThreshold).ShouldBeTrue();
    }

    #endregion

    #region Integration and Scenario Tests

    [Fact]
    public void Configuration_ForHighPerformanceScenario_ShouldConfigureCorrectly()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act - Configure for high performance (minimal telemetry overhead)
        options.Enabled = true;
        options.CaptureMiddlewareDetails = false;
        options.CaptureHandlerDetails = false;
        options.CaptureExceptionDetails = true; // Keep exceptions for debugging
        options.PacketLevelTelemetryEnabled = false;
        options.EnableStreamingMetrics = false;
        options.CapturePacketSize = false;
        options.MaxExceptionMessageLength = 100; // Shorter messages
        options.MaxStackTraceLines = 1; // Minimal stack trace

        // Assert
        options.Enabled.ShouldBeTrue();
        options.CaptureMiddlewareDetails.ShouldBeFalse();
        options.CaptureHandlerDetails.ShouldBeFalse();
        options.CaptureExceptionDetails.ShouldBeTrue();
        options.PacketLevelTelemetryEnabled.ShouldBeFalse();
        options.EnableStreamingMetrics.ShouldBeFalse();
        options.CapturePacketSize.ShouldBeFalse();
        options.MaxExceptionMessageLength.ShouldBe(100);
        options.MaxStackTraceLines.ShouldBe(1);
    }

    [Fact]
    public void Configuration_ForDebugScenario_ShouldConfigureCorrectly()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act - Configure for detailed debugging (maximum telemetry)
        options.Enabled = true;
        options.CaptureMiddlewareDetails = true;
        options.CaptureHandlerDetails = true;
        options.CaptureExceptionDetails = true;
        options.PacketLevelTelemetryEnabled = true;
        options.EnableStreamingMetrics = true;
        options.CapturePacketSize = true;
        options.MaxExceptionMessageLength = 1000; // Longer messages for debugging
        options.MaxStackTraceLines = 10; // More stack trace context
        options.PacketTelemetryBatchSize = 1; // No batching for immediate visibility

        // Assert
        options.Enabled.ShouldBeTrue();
        options.CaptureMiddlewareDetails.ShouldBeTrue();
        options.CaptureHandlerDetails.ShouldBeTrue();
        options.CaptureExceptionDetails.ShouldBeTrue();
        options.PacketLevelTelemetryEnabled.ShouldBeTrue();
        options.EnableStreamingMetrics.ShouldBeTrue();
        options.CapturePacketSize.ShouldBeTrue();
        options.MaxExceptionMessageLength.ShouldBe(1000);
        options.MaxStackTraceLines.ShouldBe(10);
        options.PacketTelemetryBatchSize.ShouldBe(1);
    }

    [Fact]
    public void Configuration_ForProductionScenario_ShouldConfigureCorrectly()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act - Configure for production (balanced telemetry)
        options.Enabled = true;
        options.CaptureMiddlewareDetails = true;
        options.CaptureHandlerDetails = true;
        options.CaptureExceptionDetails = true;
        options.PacketLevelTelemetryEnabled = false; // Performance overhead
        options.EnableStreamingMetrics = true;
        options.CapturePacketSize = false; // Performance overhead
        options.MaxExceptionMessageLength = 200; // Default balanced value
        options.MaxStackTraceLines = 3; // Default balanced value
        options.EnableHealthChecks = true;

        // Assert - Should match most default values with some specific production considerations
        options.Enabled.ShouldBeTrue();
        options.CaptureMiddlewareDetails.ShouldBeTrue();
        options.CaptureHandlerDetails.ShouldBeTrue();
        options.CaptureExceptionDetails.ShouldBeTrue();
        options.PacketLevelTelemetryEnabled.ShouldBeFalse();
        options.EnableStreamingMetrics.ShouldBeTrue();
        options.CapturePacketSize.ShouldBeFalse();
        options.MaxExceptionMessageLength.ShouldBe(200);
        options.MaxStackTraceLines.ShouldBe(3);
        options.EnableHealthChecks.ShouldBeTrue();
    }

    [Fact]
    public void Configuration_WhenDisablingAllFeatures_ShouldDisableCorrectly()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act - Disable all telemetry features
        options.Enabled = false;
        options.CaptureMiddlewareDetails = false;
        options.CaptureHandlerDetails = false;
        options.CaptureExceptionDetails = false;
        options.EnableHealthChecks = false;
        options.PacketLevelTelemetryEnabled = false;
        options.EnableStreamingMetrics = false;
        options.CapturePacketSize = false;
        options.EnableStreamingPerformanceClassification = false;

        // Assert
        options.Enabled.ShouldBeFalse();
        options.CaptureMiddlewareDetails.ShouldBeFalse();
        options.CaptureHandlerDetails.ShouldBeFalse();
        options.CaptureExceptionDetails.ShouldBeFalse();
        options.EnableHealthChecks.ShouldBeFalse();
        options.PacketLevelTelemetryEnabled.ShouldBeFalse();
        options.EnableStreamingMetrics.ShouldBeFalse();
        options.CapturePacketSize.ShouldBeFalse();
        options.EnableStreamingPerformanceClassification.ShouldBeFalse();
    }

    #endregion

    #region Property Independence Tests

    [Fact]
    public void PropertyChanges_ShouldNotAffectOtherProperties()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();
        var originalGoodThreshold = options.GoodPerformanceThreshold;
        var originalFairThreshold = options.FairPerformanceThreshold;
        var originalPatternsCount = options.SensitiveDataPatterns.Count;

        // Act - Change one property
        options.ExcellentPerformanceThreshold = 0.05;

        // Assert - Other properties should remain unchanged
        options.GoodPerformanceThreshold.ShouldBe(originalGoodThreshold);
        options.FairPerformanceThreshold.ShouldBe(originalFairThreshold);
        options.SensitiveDataPatterns.Count.ShouldBe(originalPatternsCount);
    }

    [Fact]
    public void BooleanProperties_ShouldBeIndependent()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act - Change one boolean property
        options.Enabled = false;

        // Assert - Other boolean properties should remain at their defaults
        options.CaptureMiddlewareDetails.ShouldBeTrue();
        options.CaptureHandlerDetails.ShouldBeTrue();
        options.CaptureExceptionDetails.ShouldBeTrue();
        options.EnableHealthChecks.ShouldBeTrue();
        options.EnableStreamingMetrics.ShouldBeTrue();
        options.EnableStreamingPerformanceClassification.ShouldBeTrue();
    }

    #endregion

    #region Null and Reference Tests

    [Fact]
    public void SensitiveDataPatterns_WhenSetToNull_ShouldThrowException()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act & Assert
        Should.Throw<NullReferenceException>(() => 
        {
            options.SensitiveDataPatterns = null!;
            // Try to access the property to trigger the error
            _ = options.SensitiveDataPatterns.Count;
        });
    }

    [Fact]
    public void SensitiveDataPatterns_WhenAddingNull_ShouldAllowNull()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act & Assert - Should not throw, List<string> allows null values
        Should.NotThrow(() => options.SensitiveDataPatterns.Add(null!));
        options.SensitiveDataPatterns.Any(x => x == null).ShouldBeTrue();
    }

    [Fact]
    public void SensitiveDataPatterns_WhenAddingEmptyString_ShouldAllowEmptyString()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.SensitiveDataPatterns.Add(string.Empty);

        // Assert
        options.SensitiveDataPatterns.ShouldContain(string.Empty);
    }

    #endregion
}