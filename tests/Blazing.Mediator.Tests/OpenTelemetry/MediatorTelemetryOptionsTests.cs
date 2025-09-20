using Blazing.Mediator.OpenTelemetry;

namespace Blazing.Mediator.Tests.OpenTelemetry;

/// <summary>
/// Comprehensive tests for MediatorTelemetryOptions configuration and behavior.
/// Validates all telemetry features including packet-level telemetry, performance thresholds,
/// middleware details capture, and streaming telemetry options.
/// </summary>
public class MediatorTelemetryOptionsTests
{
    [Fact]
    public void DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var options = new MediatorTelemetryOptions();

        // Assert
        Assert.True(options.Enabled);
        Assert.True(options.CaptureMiddlewareDetails);
        Assert.True(options.CaptureHandlerDetails);
        Assert.True(options.CaptureExceptionDetails);
        Assert.True(options.EnableHealthChecks);
        Assert.Equal(200, options.MaxExceptionMessageLength);
        Assert.Equal(3, options.MaxStackTraceLines);
        Assert.False(options.PacketLevelTelemetryEnabled);
        Assert.Equal(10, options.PacketTelemetryBatchSize);
        Assert.True(options.EnableStreamingMetrics);
        Assert.False(options.CapturePacketSize);
        Assert.True(options.EnableStreamingPerformanceClassification);
        Assert.Equal(0.1, options.ExcellentPerformanceThreshold);
        Assert.Equal(0.3, options.GoodPerformanceThreshold);
        Assert.Equal(0.5, options.FairPerformanceThreshold);
        Assert.NotEmpty(options.SensitiveDataPatterns);
    }

    [Fact]
    public void Enabled_WhenSetToFalse_ShouldDisableTelemetry()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.Enabled = false;

        // Assert
        Assert.False(options.Enabled);
    }

    [Fact]
    public void PacketLevelTelemetryEnabled_WhenSetToTrue_ShouldEnablePacketTelemetry()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.PacketLevelTelemetryEnabled = true;

        // Assert
        Assert.True(options.PacketLevelTelemetryEnabled);
    }

    [Fact]
    public void CaptureMiddlewareDetails_WhenSetToFalse_ShouldDisableMiddlewareCapture()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.CaptureMiddlewareDetails = false;

        // Assert
        Assert.False(options.CaptureMiddlewareDetails);
    }

    [Fact]
    public void MaxExceptionMessageLength_WhenSetToValidValue_ShouldUpdateCorrectly()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.MaxExceptionMessageLength = 500;

        // Assert
        Assert.Equal(500, options.MaxExceptionMessageLength);
    }

    [Fact]
    public void PacketTelemetryBatchSize_WhenSetToValidValue_ShouldUpdateCorrectly()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.PacketTelemetryBatchSize = 50;

        // Assert
        Assert.Equal(50, options.PacketTelemetryBatchSize);
    }

    [Fact]
    public void EnableStreamingMetrics_WhenSetToFalse_ShouldDisableStreamingMetrics()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.EnableStreamingMetrics = false;

        // Assert
        Assert.False(options.EnableStreamingMetrics);
    }

    [Fact]
    public void CapturePacketSize_WhenSetToTrue_ShouldEnablePacketSizeCapture()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.CapturePacketSize = true;

        // Assert
        Assert.True(options.CapturePacketSize);
    }

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
        Assert.Equal(0.05, options.ExcellentPerformanceThreshold);
        Assert.Equal(0.2, options.GoodPerformanceThreshold);
        Assert.Equal(0.4, options.FairPerformanceThreshold);
    }

    [Fact]
    public void SensitiveDataPatterns_WhenModified_ShouldAllowCustomization()
    {
        // Arrange
        var options = new MediatorTelemetryOptions();

        // Act
        options.SensitiveDataPatterns.Add("custom-pattern");

        // Assert
        Assert.Contains("custom-pattern", options.SensitiveDataPatterns);
        Assert.Contains("password", options.SensitiveDataPatterns);
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
        Assert.Equal(value, options.CaptureHandlerDetails);
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
        Assert.Equal(value, options.CaptureExceptionDetails);
    }
}