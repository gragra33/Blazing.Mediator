## Release 1.0.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
BLAZMED001 | Blazing.Mediator | Error | Open generic handler detected; source generation does not support open generic handlers
BLAZMED002 | Blazing.Mediator | Warning | Telemetry is enabled but no telemetry sink is registered
BLAZMED003 | Blazing.Mediator | Info | Source generation completed successfully
BLAZMED004 | Blazing.Mediator | Info | No handlers found; source generation will be skipped
BLAZMED013 | Blazing.Mediator | Warning | Middleware has type parameter constraints not satisfied by the request type
BLAZMED014 | Blazing.Mediator | Warning | Middleware does not have an Order property; default order (0) will be used
BLAZMED015 | Blazing.Mediator | Warning | Subscriber is not registered in the DI container
BLAZMED016 | Blazing.Mediator | Error | Cannot resolve subscriber from the DI container
BLAZMED017 | Blazing.Mediator | Warning | Stream request has no middleware registered
BLAZMED018 | Blazing.Mediator | Error | Cannot resolve stream middleware from the DI container
BLAZMED019 | Blazing.Mediator | Warning | Type may be trimmed in AOT scenarios; consider adding DynamicallyAccessedMembers attribute
BLAZMED020 | Blazing.Mediator | Warning | AOT compatibility attributes are not applied to the type
BLAZMED021 | Blazing.Mediator | Error | Performance target not met (disabled by default; opt-in for benchmark builds)
