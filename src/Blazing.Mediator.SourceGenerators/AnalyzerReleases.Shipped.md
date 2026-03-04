## Release 1.0.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
BLAZMED001 | Performance | Warning | Handler not found for request
BLAZMED002 | Performance | Warning | Multiple handlers registered for request
BLAZMED003 | Design | Info | Consider using IRequest<TResponse> instead of IRequest
BLAZMED004 | Design | Warning | Request handler must implement IRequestHandler
BLAZMED013 | Design | Warning | Notification handler must implement INotificationHandler
BLAZMED014 | Design | Info | Consider using records for immutable requests
BLAZMED015 | Performance | Warning | Avoid using reflection in hot paths
BLAZMED016 | Performance | Info | Consider using source-generated dispatch
BLAZMED017 | Design | Warning | Middleware order affects execution
BLAZMED018 | Design | Info | Consider adding telemetry to handlers
BLAZMED019 | Design | Warning | Stream handlers should implement IStreamRequestHandler
BLAZMED020 | Performance | Warning | Avoid sync-over-async patterns
BLAZMED021 | Design | Info | Consider using CancellationToken in all async methods
