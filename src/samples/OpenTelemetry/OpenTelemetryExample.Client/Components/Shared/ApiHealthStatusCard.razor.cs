using Microsoft.AspNetCore.Components;

namespace OpenTelemetryExample.Client.Components.Shared;

public partial class ApiHealthStatusCard : ComponentBase
{
    [Parameter] public bool? DataSource { get; set; }
    [Parameter] public bool Loading { get; set; }
    private bool _apiHealthLoading => Loading;
}
