using Microsoft.AspNetCore.Components;

namespace OpenTelemetryExample.Client.Components.Shared;

/// <summary>
/// Represents a card component that displays the health status of an API.
/// </summary>
public partial class ApiHealthStatusCard : ComponentBase
{
    /// <summary>
    /// Gets or sets the data source indicating the health status of the API.
    /// </summary>
    [Parameter] public bool? DataSource { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the health status is currently loading.
    /// </summary>
    [Parameter] public bool Loading { get; set; }

    /// <summary>
    /// Gets a value indicating whether the API health status is loading.
    /// </summary>
    private bool _apiHealthLoading => Loading;
}
