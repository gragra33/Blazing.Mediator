using System.Diagnostics.Metrics;

namespace Blazing.Mediator;

/// <summary>
/// Centralised OpenTelemetry metric instruments for Blazing.Mediator.
/// All instruments are initialised once against <see cref="Mediator.Meter"/> and shared
/// across the streaming pipeline and telemetry context.
/// </summary>
/// <remarks>
/// Keeping instruments here instead of inline on <see cref="Mediator"/> removes the
/// <c>internal</c> coupling that previously forced the streaming telemetry context
/// to access private/internal members of the <see cref="Mediator"/> partial class.
/// </remarks>
internal static class MediatorMetrics
{
    // ── Unit constants ─────────────────────────────────────────────────────
    private const string MillisecondsUnit = "ms";
    private const string ItemsPerSecondUnit = "items/sec";

    // ── Streaming — duration / outcome ────────────────────────────────────
    internal static readonly Histogram<double> StreamDurationHistogram =
        Mediator.Meter.CreateHistogram<double>(
            "mediator.stream.duration",
            unit: MillisecondsUnit,
            description: "Duration of mediator stream operations");

    internal static readonly Counter<long> StreamSuccessCounter =
        Mediator.Meter.CreateCounter<long>(
            "mediator.stream.success",
            description: "Number of successful mediator stream operations");

    internal static readonly Counter<long> StreamFailureCounter =
        Mediator.Meter.CreateCounter<long>(
            "mediator.stream.failure",
            description: "Number of failed mediator stream operations");

    // ── Streaming — throughput / latency ──────────────────────────────────
    internal static readonly Histogram<double> StreamThroughputHistogram =
        Mediator.Meter.CreateHistogram<double>(
            "mediator.stream.throughput",
            unit: ItemsPerSecondUnit,
            description: "Throughput of mediator stream operations");

    internal static readonly Histogram<double> StreamTtfbHistogram =
        Mediator.Meter.CreateHistogram<double>(
            "mediator.stream.ttfb",
            unit: MillisecondsUnit,
            description: "Time to first byte for mediator stream operations");

    // ── Streaming — packet-level metrics ─────────────────────────────────
    internal static readonly Counter<long> StreamPacketCounter =
        Mediator.Meter.CreateCounter<long>(
            "mediator.stream.packet.count",
            description: "Number of packets processed in stream operations");

    internal static readonly Histogram<double> StreamPacketProcessingTimeHistogram =
        Mediator.Meter.CreateHistogram<double>(
            "mediator.stream.packet.processing_time",
            unit: MillisecondsUnit,
            description: "Processing time for individual stream packets");

    internal static readonly Histogram<double> StreamInterPacketTimeHistogram =
        Mediator.Meter.CreateHistogram<double>(
            "mediator.stream.inter_packet_time",
            unit: MillisecondsUnit,
            description: "Time between consecutive stream packets");

    internal static readonly Histogram<double> StreamPacketJitterHistogram =
        Mediator.Meter.CreateHistogram<double>(
            "mediator.stream.packet.jitter",
            unit: MillisecondsUnit,
            description: "Jitter in stream packet timing");

    // ── Health check ──────────────────────────────────────────────────────
    internal static readonly Counter<long> TelemetryHealthCounter =
        Mediator.Meter.CreateCounter<long>(
            "mediator.telemetry.health",
            description: "Health check counter for telemetry system");

    // ── Send (request dispatch) ────────────────────────────────────────────
    internal static readonly Histogram<double> SendDurationHistogram =
        Mediator.Meter.CreateHistogram<double>(
            "mediator.send.duration",
            unit: MillisecondsUnit,
            description: "Duration of mediator send operations");

    internal static readonly Counter<long> SendSuccessCounter =
        Mediator.Meter.CreateCounter<long>(
            "mediator.send.success",
            description: "Number of successful mediator send operations");

    internal static readonly Counter<long> SendFailureCounter =
        Mediator.Meter.CreateCounter<long>(
            "mediator.send.failure",
            description: "Number of failed mediator send operations");

    // ── Publish (notification dispatch) ────────────────────────────────────
    internal static readonly Histogram<double> PublishDurationHistogram =
        Mediator.Meter.CreateHistogram<double>(
            "mediator.publish.duration",
            unit: MillisecondsUnit,
            description: "Duration of mediator publish operations");

    internal static readonly Counter<long> PublishSuccessCounter =
        Mediator.Meter.CreateCounter<long>(
            "mediator.publish.success",
            description: "Number of successful mediator publish operations");

    internal static readonly Counter<long> PublishFailureCounter =
        Mediator.Meter.CreateCounter<long>(
            "mediator.publish.failure",
            description: "Number of failed mediator publish operations");

    internal static readonly Histogram<double> PublishSubscriberDurationHistogram =
        Mediator.Meter.CreateHistogram<double>(
            "mediator.publish.subscriber.duration",
            unit: MillisecondsUnit,
            description: "Duration of individual subscriber notification processing");

    internal static readonly Counter<long> PublishSubscriberSuccessCounter =
        Mediator.Meter.CreateCounter<long>(
            "mediator.publish.subscriber.success",
            description: "Number of successful subscriber notifications");

    internal static readonly Counter<long> PublishSubscriberFailureCounter =
        Mediator.Meter.CreateCounter<long>(
            "mediator.publish.subscriber.failure",
            description: "Number of failed subscriber notifications");

    internal static readonly Counter<long> PublishPartialFailureCounter =
        Mediator.Meter.CreateCounter<long>(
            "mediator.publish.partial_failure",
            description: "Number of notifications with partial handler failures");

    internal static readonly Counter<long> PublishTotalFailureCounter =
        Mediator.Meter.CreateCounter<long>(
            "mediator.publish.total_failure",
            description: "Number of notifications where all handlers failed");
}
