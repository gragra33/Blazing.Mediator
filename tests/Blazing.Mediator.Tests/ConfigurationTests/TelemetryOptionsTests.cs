using Blazing.Mediator.Configuration;

namespace Blazing.Mediator.Tests.ConfigurationTests;

/// <summary>
/// Tests for TelemetryOptions notification-specific configuration properties.
/// </summary>
public class TelemetryOptionsTests
{
    [Fact]
    public void TelemetryOptions_Should_Have_Default_Notification_Settings()
    {
        // Act
        var options = new TelemetryOptions();
        
        // Assert
        options.CaptureNotificationHandlerDetails.ShouldBeTrue();
        options.CreateHandlerChildSpans.ShouldBeTrue();
        options.CaptureSubscriberMetrics.ShouldBeTrue();
        options.CaptureNotificationMiddlewareDetails.ShouldBeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CaptureNotificationHandlerDetails_WhenSetToValue_ShouldUpdateCorrectly(bool value)
    {
        // Arrange
        var options = new TelemetryOptions
        {
            // Act
            CaptureNotificationHandlerDetails = value
        };

        // Assert
        options.CaptureNotificationHandlerDetails.ShouldBe(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CreateHandlerChildSpans_WhenSetToValue_ShouldUpdateCorrectly(bool value)
    {
        // Arrange
        var options = new TelemetryOptions
        {
            // Act
            CreateHandlerChildSpans = value
        };

        // Assert
        options.CreateHandlerChildSpans.ShouldBe(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CaptureSubscriberMetrics_WhenSetToValue_ShouldUpdateCorrectly(bool value)
    {
        // Arrange
        var options = new TelemetryOptions
        {
            // Act
            CaptureSubscriberMetrics = value
        };

        // Assert
        options.CaptureSubscriberMetrics.ShouldBe(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CaptureNotificationMiddlewareDetails_WhenSetToValue_ShouldUpdateCorrectly(bool value)
    {
        // Arrange
        var options = new TelemetryOptions
        {
            // Act
            CaptureNotificationMiddlewareDetails = value
        };

        // Assert
        options.CaptureNotificationMiddlewareDetails.ShouldBe(value);
    }

    [Fact]
    public void NotificationTelemetryOptions_ShouldBeIndependentOfOtherOptions()
    {
        // Arrange
        var options = new TelemetryOptions();
        var originalEnabled = options.Enabled;
        var originalCaptureHandlerDetails = options.CaptureHandlerDetails;

        // Act
        options.CaptureNotificationHandlerDetails = false;
        options.CreateHandlerChildSpans = false;

        // Assert - Other properties should remain unchanged
        options.Enabled.ShouldBe(originalEnabled);
        options.CaptureHandlerDetails.ShouldBe(originalCaptureHandlerDetails);
    }

    [Fact]
    public void NotificationTelemetryConfiguration_ForHighPerformanceScenario_ShouldConfigureCorrectly()
    {
        // Arrange
        var options = new TelemetryOptions
        {
            // Act - Configure for high performance (minimal telemetry overhead)
            CaptureNotificationHandlerDetails = false,
            CreateHandlerChildSpans = false,
            CaptureSubscriberMetrics = false,
            CaptureNotificationMiddlewareDetails = false
        };

        // Assert
        options.CaptureNotificationHandlerDetails.ShouldBeFalse();
        options.CreateHandlerChildSpans.ShouldBeFalse();
        options.CaptureSubscriberMetrics.ShouldBeFalse();
        options.CaptureNotificationMiddlewareDetails.ShouldBeFalse();
    }

    [Fact]
    public void NotificationTelemetryConfiguration_ForDebuggingScenario_ShouldConfigureCorrectly()
    {
        // Arrange
        var options = new TelemetryOptions
        {
            // Act - Configure for detailed debugging (maximum telemetry)
            CaptureNotificationHandlerDetails = true,
            CreateHandlerChildSpans = true,
            CaptureSubscriberMetrics = true,
            CaptureNotificationMiddlewareDetails = true
        };

        // Assert
        options.CaptureNotificationHandlerDetails.ShouldBeTrue();
        options.CreateHandlerChildSpans.ShouldBeTrue();
        options.CaptureSubscriberMetrics.ShouldBeTrue();
        options.CaptureNotificationMiddlewareDetails.ShouldBeTrue();
    }

    #region Validation Tests

    [Fact]
    public void Validate_WithValidConfiguration_ShouldReturnNoErrors()
    {
        // Arrange
        var options = new TelemetryOptions();

        // Act
        var errors = options.Validate();

        // Assert
        errors.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithNegativeMaxExceptionMessageLength_ShouldReturnError(int length)
    {
        // Arrange
        var options = new TelemetryOptions
        {
            MaxExceptionMessageLength = length
        };

        // Act
        var errors = options.Validate();

        // Assert
        errors.ShouldContain("MaxExceptionMessageLength cannot be negative.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Validate_WithNegativeMaxStackTraceLines_ShouldReturnError(int lines)
    {
        // Arrange
        var options = new TelemetryOptions
        {
            MaxStackTraceLines = lines
        };

        // Act
        var errors = options.Validate();

        // Assert
        errors.ShouldContain("MaxStackTraceLines cannot be negative.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidPacketTelemetryBatchSize_ShouldReturnError(int batchSize)
    {
        // Arrange
        var options = new TelemetryOptions
        {
            PacketTelemetryBatchSize = batchSize
        };

        // Act
        var errors = options.Validate();

        // Assert
        errors.ShouldContain("PacketTelemetryBatchSize must be at least 1.");
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    [InlineData(2.0)]
    public void Validate_WithInvalidPerformanceThresholds_ShouldReturnErrors(double threshold)
    {
        // Arrange
        var options = new TelemetryOptions
        {
            ExcellentPerformanceThreshold = threshold,
            GoodPerformanceThreshold = threshold,
            FairPerformanceThreshold = threshold
        };

        // Act
        var errors = options.Validate();

        // Assert
        errors.ShouldContain("ExcellentPerformanceThreshold must be between 0 and 1.");
        errors.ShouldContain("GoodPerformanceThreshold must be between 0 and 1.");
        errors.ShouldContain("FairPerformanceThreshold must be between 0 and 1.");
    }

    [Fact]
    public void Validate_WithIncorrectThresholdOrder_ShouldReturnErrors()
    {
        // Arrange
        var options = new TelemetryOptions
        {
            ExcellentPerformanceThreshold = 0.8,
            GoodPerformanceThreshold = 0.5,
            FairPerformanceThreshold = 0.3
        };

        // Act
        var errors = options.Validate();

        // Assert
        errors.ShouldContain("ExcellentPerformanceThreshold must be less than GoodPerformanceThreshold.");
        errors.ShouldContain("GoodPerformanceThreshold must be less than FairPerformanceThreshold.");
    }

    [Fact]
    public void Validate_WithNullSensitiveDataPatterns_ShouldReturnError()
    {
        // Arrange
        var options = new TelemetryOptions
        {
            SensitiveDataPatterns = null!
        };

        // Act
        var errors = options.Validate();

        // Assert
        errors.ShouldContain("SensitiveDataPatterns cannot be null.");
    }

    [Fact]
    public void ValidateAndThrow_WithValidConfiguration_ShouldNotThrow()
    {
        // Arrange
        var options = new TelemetryOptions();

        // Act & Assert
        Should.NotThrow(() => options.ValidateAndThrow());
    }

    [Fact]
    public void ValidateAndThrow_WithInvalidConfiguration_ShouldThrowArgumentException()
    {
        // Arrange
        var options = new TelemetryOptions
        {
            MaxExceptionMessageLength = -1
        };

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => options.ValidateAndThrow());
        exception.Message.ShouldContain("Invalid TelemetryOptions configuration");
        exception.Message.ShouldContain("MaxExceptionMessageLength cannot be negative");
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateExactCopy()
    {
        // Arrange
        var original = new TelemetryOptions
        {
            Enabled = false,
            CaptureMiddlewareDetails = false,
            CaptureHandlerDetails = false,
            CaptureExceptionDetails = false,
            EnableHealthChecks = false,
            MaxExceptionMessageLength = 100,
            MaxStackTraceLines = 5,
            PacketLevelTelemetryEnabled = true,
            PacketTelemetryBatchSize = 25,
            EnableStreamingMetrics = false,
            CapturePacketSize = true,
            EnableStreamingPerformanceClassification = false,
            ExcellentPerformanceThreshold = 0.05,
            GoodPerformanceThreshold = 0.25,
            FairPerformanceThreshold = 0.75,
            CaptureNotificationHandlerDetails = false,
            CreateHandlerChildSpans = false,
            CaptureSubscriberMetrics = false,
            CaptureNotificationMiddlewareDetails = false,
            SensitiveDataPatterns = ["test", "custom"]
        };

        // Act
        var clone = original.Clone();

        // Assert
        clone.ShouldNotBeSameAs(original);
        clone.Enabled.ShouldBe(original.Enabled);
        clone.CaptureMiddlewareDetails.ShouldBe(original.CaptureMiddlewareDetails);
        clone.CaptureHandlerDetails.ShouldBe(original.CaptureHandlerDetails);
        clone.CaptureExceptionDetails.ShouldBe(original.CaptureExceptionDetails);
        clone.EnableHealthChecks.ShouldBe(original.EnableHealthChecks);
        clone.MaxExceptionMessageLength.ShouldBe(original.MaxExceptionMessageLength);
        clone.MaxStackTraceLines.ShouldBe(original.MaxStackTraceLines);
        clone.PacketLevelTelemetryEnabled.ShouldBe(original.PacketLevelTelemetryEnabled);
        clone.PacketTelemetryBatchSize.ShouldBe(original.PacketTelemetryBatchSize);
        clone.EnableStreamingMetrics.ShouldBe(original.EnableStreamingMetrics);
        clone.CapturePacketSize.ShouldBe(original.CapturePacketSize);
        clone.EnableStreamingPerformanceClassification.ShouldBe(original.EnableStreamingPerformanceClassification);
        clone.ExcellentPerformanceThreshold.ShouldBe(original.ExcellentPerformanceThreshold);
        clone.GoodPerformanceThreshold.ShouldBe(original.GoodPerformanceThreshold);
        clone.FairPerformanceThreshold.ShouldBe(original.FairPerformanceThreshold);
        clone.CaptureNotificationHandlerDetails.ShouldBe(original.CaptureNotificationHandlerDetails);
        clone.CreateHandlerChildSpans.ShouldBe(original.CreateHandlerChildSpans);
        clone.CaptureSubscriberMetrics.ShouldBe(original.CaptureSubscriberMetrics);
        clone.CaptureNotificationMiddlewareDetails.ShouldBe(original.CaptureNotificationMiddlewareDetails);
        clone.SensitiveDataPatterns.ShouldNotBeSameAs(original.SensitiveDataPatterns);
        clone.SensitiveDataPatterns.ShouldBe(original.SensitiveDataPatterns);
    }

    #endregion

    #region Property Tests

    [Theory]
    [InlineData(true, true, true, true, true)]
    [InlineData(false, false, false, false, false)]
    [InlineData(true, false, false, false, false)]
    [InlineData(false, true, false, false, false)] // Fixed: If Enabled=false, result should always be false
    public void IsEnabled_ShouldReturnCorrectValue(bool enabled, bool captureMiddleware, bool captureHandler, 
        bool captureNotificationHandler, bool expected)
    {
        // Arrange
        var options = new TelemetryOptions
        {
            Enabled = enabled,
            CaptureMiddlewareDetails = captureMiddleware,
            CaptureHandlerDetails = captureHandler,
            CaptureNotificationHandlerDetails = captureNotificationHandler,
            EnableStreamingMetrics = false,
            EnableHealthChecks = false
        };

        // Act
        var result = options.IsEnabled;

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(true, true, true, true, true, true)]
    [InlineData(false, false, false, false, false, false)]
    [InlineData(true, false, false, false, false, false)]
    [InlineData(true, true, false, false, false, true)]
    public void IsNotificationTelemetryEnabled_ShouldReturnCorrectValue(bool enabled, bool captureHandlerDetails,
        bool createChildSpans, bool captureSubscribers, bool captureMiddleware, bool expected)
    {
        // Arrange
        var options = new TelemetryOptions
        {
            Enabled = enabled,
            CaptureNotificationHandlerDetails = captureHandlerDetails,
            CreateHandlerChildSpans = createChildSpans,
            CaptureSubscriberMetrics = captureSubscribers,
            CaptureNotificationMiddlewareDetails = captureMiddleware
        };

        // Act
        var result = options.IsNotificationTelemetryEnabled;

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(true, true, false, true)]
    [InlineData(false, false, false, false)]
    [InlineData(true, false, true, true)]
    [InlineData(true, false, false, false)]
    public void IsStreamingTelemetryEnabled_ShouldReturnCorrectValue(bool enabled, bool streamingMetrics, 
        bool packetLevel, bool expected)
    {
        // Arrange
        var options = new TelemetryOptions
        {
            Enabled = enabled,
            EnableStreamingMetrics = streamingMetrics,
            PacketLevelTelemetryEnabled = packetLevel
        };

        // Act
        var result = options.IsStreamingTelemetryEnabled;

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region Factory Method Tests

    [Fact]
    public void Development_ShouldReturnDevelopmentConfiguration()
    {
        // Act
        var options = TelemetryOptions.Development();

        // Assert
        options.Enabled.ShouldBeTrue();
        options.CaptureMiddlewareDetails.ShouldBeTrue();
        options.CaptureHandlerDetails.ShouldBeTrue();
        options.CaptureExceptionDetails.ShouldBeTrue();
        options.EnableHealthChecks.ShouldBeTrue();
        options.MaxExceptionMessageLength.ShouldBe(500);
        options.MaxStackTraceLines.ShouldBe(10);
        options.PacketLevelTelemetryEnabled.ShouldBeTrue();
        options.PacketTelemetryBatchSize.ShouldBe(5);
        options.EnableStreamingMetrics.ShouldBeTrue();
        options.CapturePacketSize.ShouldBeTrue();
        options.EnableStreamingPerformanceClassification.ShouldBeTrue();
        options.CaptureNotificationHandlerDetails.ShouldBeTrue();
        options.CreateHandlerChildSpans.ShouldBeTrue();
        options.CaptureSubscriberMetrics.ShouldBeTrue();
        options.CaptureNotificationMiddlewareDetails.ShouldBeTrue();
    }

    [Fact]
    public void Production_ShouldReturnProductionConfiguration()
    {
        // Act
        var options = TelemetryOptions.Production();

        // Assert
        options.Enabled.ShouldBeTrue();
        options.CaptureMiddlewareDetails.ShouldBeFalse();
        options.CaptureHandlerDetails.ShouldBeTrue();
        options.CaptureExceptionDetails.ShouldBeTrue();
        options.EnableHealthChecks.ShouldBeTrue();
        options.MaxExceptionMessageLength.ShouldBe(200);
        options.MaxStackTraceLines.ShouldBe(3);
        options.PacketLevelTelemetryEnabled.ShouldBeFalse();
        options.PacketTelemetryBatchSize.ShouldBe(20);
        options.EnableStreamingMetrics.ShouldBeTrue();
        options.CapturePacketSize.ShouldBeFalse();
        options.EnableStreamingPerformanceClassification.ShouldBeFalse();
        options.CaptureNotificationHandlerDetails.ShouldBeTrue();
        options.CreateHandlerChildSpans.ShouldBeFalse();
        options.CaptureSubscriberMetrics.ShouldBeFalse();
        options.CaptureNotificationMiddlewareDetails.ShouldBeFalse();
    }

    [Fact]
    public void Disabled_ShouldReturnDisabledConfiguration()
    {
        // Act
        var options = TelemetryOptions.Disabled();

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
        options.CaptureNotificationHandlerDetails.ShouldBeFalse();
        options.CreateHandlerChildSpans.ShouldBeFalse();
        options.CaptureSubscriberMetrics.ShouldBeFalse();
        options.CaptureNotificationMiddlewareDetails.ShouldBeFalse();
        options.IsEnabled.ShouldBeFalse();
    }

    [Fact]
    public void Minimal_ShouldReturnMinimalConfiguration()
    {
        // Act
        var options = TelemetryOptions.Minimal();

        // Assert
        options.Enabled.ShouldBeTrue();
        options.CaptureMiddlewareDetails.ShouldBeFalse();
        options.CaptureHandlerDetails.ShouldBeFalse();
        options.CaptureExceptionDetails.ShouldBeTrue();
        options.EnableHealthChecks.ShouldBeTrue();
        options.MaxExceptionMessageLength.ShouldBe(100);
        options.MaxStackTraceLines.ShouldBe(2);
        options.PacketLevelTelemetryEnabled.ShouldBeFalse();
        options.EnableStreamingMetrics.ShouldBeFalse();
        options.CaptureNotificationHandlerDetails.ShouldBeFalse();
        options.CreateHandlerChildSpans.ShouldBeFalse();
        options.CaptureSubscriberMetrics.ShouldBeFalse();
        options.CaptureNotificationMiddlewareDetails.ShouldBeFalse();
    }

    [Fact]
    public void NotificationOnly_ShouldReturnNotificationOnlyConfiguration()
    {
        // Act
        var options = TelemetryOptions.NotificationOnly();

        // Assert
        options.Enabled.ShouldBeTrue();
        options.CaptureMiddlewareDetails.ShouldBeFalse();
        options.CaptureHandlerDetails.ShouldBeFalse();
        options.CaptureExceptionDetails.ShouldBeTrue();
        options.EnableHealthChecks.ShouldBeFalse();
        options.PacketLevelTelemetryEnabled.ShouldBeFalse();
        options.EnableStreamingMetrics.ShouldBeFalse();
        options.CaptureNotificationHandlerDetails.ShouldBeTrue();
        options.CreateHandlerChildSpans.ShouldBeTrue();
        options.CaptureSubscriberMetrics.ShouldBeTrue();
        options.CaptureNotificationMiddlewareDetails.ShouldBeTrue();
        options.IsNotificationTelemetryEnabled.ShouldBeTrue();
        options.IsStreamingTelemetryEnabled.ShouldBeFalse();
    }

    [Fact]
    public void StreamingOnly_ShouldReturnStreamingOnlyConfiguration()
    {
        // Act
        var options = TelemetryOptions.StreamingOnly();

        // Assert
        options.Enabled.ShouldBeTrue();
        options.CaptureMiddlewareDetails.ShouldBeFalse();
        options.CaptureHandlerDetails.ShouldBeFalse();
        options.CaptureExceptionDetails.ShouldBeTrue();
        options.EnableHealthChecks.ShouldBeFalse();
        options.PacketLevelTelemetryEnabled.ShouldBeTrue();
        options.PacketTelemetryBatchSize.ShouldBe(10);
        options.EnableStreamingMetrics.ShouldBeTrue();
        options.CapturePacketSize.ShouldBeTrue();
        options.EnableStreamingPerformanceClassification.ShouldBeTrue();
        options.CaptureNotificationHandlerDetails.ShouldBeFalse();
        options.CreateHandlerChildSpans.ShouldBeFalse();
        options.CaptureSubscriberMetrics.ShouldBeFalse();
        options.CaptureNotificationMiddlewareDetails.ShouldBeFalse();
        options.IsNotificationTelemetryEnabled.ShouldBeFalse();
        options.IsStreamingTelemetryEnabled.ShouldBeTrue();
    }

    #endregion
}