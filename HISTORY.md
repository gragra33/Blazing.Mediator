# Version History

### V1.8.0 - 19 September, 2025

-   **OpenTelemetry Integration**: Full observability support with distributed tracing, metrics collection, and performance monitoring for enhanced debugging and monitoring capabilities
-   **Fluent Configuration API**: New modern fluent configuration approach using `builder.Services.AddMediator(config => { ... })` for improved type safety and enhanced functionality
-   **Legacy Method Deprecation**: Marked older `AddMediator()` and `AddMediatorFromLoadedAssemblies()` methods with boolean parameters as obsolete while maintaining backward compatibility during transition period
-   **OpenTelemetryExample Sample**: New comprehensive sample project demonstrating OpenTelemetry integration with web API server, Blazor client, and .NET Aspire support for modern cloud-native applications
-   **Enhanced Documentation**: Updated all documentation with new fluent configuration examples and comprehensive migration guidance from legacy registration methods
-   **Improved Developer Experience**: Streamlined configuration process with better IntelliSense support and compile-time validation through fluent API design

### V1.7.0 - 19 September, 2025

-   **Type-Constrained Middleware Support**: Enhanced middleware pipeline with generic type constraint validation for selective middleware execution based on interface types
-   **Request Middleware Type Constraints**: Added support for constraining request middleware to specific interface types (e.g., `ICommand`, `IQuery`) for precise middleware targeting
-   **Notification Middleware Type Constraints**: Extended type constraint support to notification middleware for selective notification processing based on interface implementations
-   **Enhanced CQRS Interface Support**: Improved middleware pipeline to respect typed constraints for compile-time middleware applicability
-   **TypedMiddlewareExample Sample**: New comprehensive sample project demonstrating type-constrained middleware with clear ICommand/IQuery distinction and selective validation
-   **Generic Constraint Validation**: Advanced generic type constraint checking with support for complex constraint scenarios including class, struct, and interface constraints
-   **Enhanced Type Safety**: Compile-time enforcement of middleware applicability through generic type constraints reducing runtime errors and improving performance
-   **Comprehensive Test Coverage**: Extensive test suite for type constraint validation covering edge cases, constraint inheritance, and complex generic scenarios
-   **Enhanced Sample Projects**: Updated ECommerce.Api and UserManagement.Api with comprehensive mediator statistics endpoints for runtime monitoring and analysis

### V1.6.2 - 18 September, 2025

-   **Enhanced Handler Analysis**: Updated `MediatorStatistics.AnalyzeQueries()` and `AnalyzeCommands()` with comprehensive handler detection and status reporting
-   **Handler Status Tracking**: New `HandlerStatus` enum with ASCII markers (`+` = found, `!` = missing, `#` = multiple) for easy visual identification
-   **Primary Interface Detection**: Enhanced `QueryCommandAnalysis` with `PrimaryInterface` property showing the main interface implemented (IQuery, ICommand, IRequest)
-   **IResult Detection**: New `IsResultType` property identifies ASP.NET Core IResult implementations for better API analysis
-   **Improved Statistics Display**: Enhanced console output in sample projects with multi-line, detailed analysis format for better readability
-   **Comprehensive Test Coverage**: Updated tests to cover all new `QueryCommandAnalysis` properties with full validation and edge case testing
-   **Documentation Enhancement**: Updated `MEDIATOR_PATTERN_GUIDE.md` with detailed `QueryCommandAnalysis` property table and enhanced example outputs

### V1.6.1 - 15 September, 2025

-   **MediatorStatistics Analysis**: New `MediatorStatistics.AnalyzeQueries()` and `AnalyzeCommands()` methods for comprehensive CQRS type discovery and analysis
-   **Runtime Statistics**: Enhanced `ReportStatistics()` functionality with automatic execution tracking via `IncrementQuery`, `IncrementCommand`, and `IncrementNotification`
-   **Statistics Monitoring**: Built-in performance monitoring and usage analytics with a flexible `IStatisticsRenderer` system for custom output formats
-   **Application Insights**: Complete application discovery capabilities perfect for health checks, monitoring dashboards, and development tooling

### V1.6.0 - 12 September, 2025

-   **Enhanced Auto-Discovery**: `AddMediator` now separates request and notification middleware auto-discovery with new `discoverMiddleware` and `discoverNotificationMiddleware` parameters for granular control
-   **New Middleware Analysis**: Added `AnalyzeMiddleware` method to both `IMiddlewarePipelineInspector` and `INotificationMiddlewarePipelineInspector` for advanced pipeline debugging and monitoring
-   **Pipeline Enhancement**: Updated `NotificationPipelineBuilder` with improved middleware management and analysis capabilities
-   **Enhanced Testing**: Comprehensive new test coverage for `AnalyzeMiddleware` functionality and middleware discovery patterns
-   **Simple Notification Example**: New `SimpleNotificationExample` sample project demonstrating recommended scoped notification patterns with clear documentation and best practices
-   **CQRS Naming Support**: Added `IQuery`, `IQueryHandler`, `ICommand`, and `ICommandHandler` interfaces as semantic wrappers around `IRequest` and `IRequestHandler` for clearer CQRS pattern implementation

### V1.5.0 - 31 July, 2025

-   **Expanded Middleware Order Range**: Expanded ordered middleware range from -999/999 to int.MinValue/int.MaxValue for greater flexibility
-   **Enhanced Pipeline Inspection**: Enhanced `IMiddlewarePipelineInspector` with sample usage in `MiddlewareExample` sample project
-   **New MiddlewareExample Project**: New `MiddlewareExample` project to demonstrate the simple yet powerful pipeline capabilities - includes `ErrorHandlingMiddleware` & `ValidationMiddleware` implementations. Documentation included.

### V1.4.2 - 26 July, 2025

-   **Middleware Order Fix**: Fixed middleware order to follow registration order rather than `Order` property for more predictable behavior
-   **Enhanced Testing**: Updated tests with stricter middleware order validation checks
-   **New Examples Project**: Added comprehensive `Blazing.Mediator.Examples` project with detailed README showcasing all features and MediatR migration patterns
-   **Benchmarking**: New `Blazing.Mediator.Benchmarks` project for performance testing and optimization

### V1.4.1 - 16 July, 2025

-   **Missing Interface Fix**: Added missing `IConditionalStreamRequestMiddleware` interface for conditional stream middleware support
-   **ECommerce.Api Enhancement**: Minor fix to `ECommerce.Api.Controllers.SimulateBulkOrder` method for improved bulk order simulation
-   **PowerShell Testing Script**: Added new `test-notifications-endpoints.ps1` script for comprehensive notification system testing and demonstration
-   **Documentation Updates**: Updated [Notification System Guide](docs/NOTIFICATION_GUIDE.md) with detailed PowerShell script usage instructions and automated testing workflows

### V1.4.0 - 16 July, 2025

-   **Notification System**: Added comprehensive notification system with observer pattern implementation
-   **Event-Driven Architecture**: Introduced `INotification` and `INotificationHandler<T>` for domain event publishing and handling
-   **Subscription Management**: Added `INotificationSubscriber` interface for managing notification subscription lifecycle
-   **Notification Middleware**: Full middleware pipeline support for notification processing with cross-cutting concerns
-   **Complete Test Coverage**: Comprehensive test coverage for notification infrastructure with extensive test suite
-   **Notification Documentation**: New [Notification System Guide](docs/NOTIFICATION_GUIDE.md) with comprehensive examples and patterns
-   **Enhanced Samples**: Updated ECommerce.Api sample with notification system, domain events, and background services

### V1.3.0 - 13 July, 2025

-   **Native Streaming Support**: Added comprehensive streaming capabilities with `IStreamRequest<T>` and `IStreamRequestHandler<T,TResponse>`
-   **Stream Middleware Pipeline**: Full middleware support for streaming requests with `IStreamRequestMiddleware<TRequest,TResponse>`
-   **Memory-Efficient Processing**: Stream large datasets with `IAsyncEnumerable<T>` without loading entire datasets into memory
-   **Multiple Streaming Patterns**: Support for JSON streaming, Server-Sent Events (SSE), and real-time data feeds
-   **Comprehensive Streaming Sample**: New Streaming.Api sample with 6 different streaming implementations across multiple Blazor render modes, APIs (Swagger)
-   **Complete Test Coverage**: 100% test coverage for streaming middleware infrastructure with comprehensive test suite
-   **Streaming Documentation**: New [Mediator Streaming Guide](docs/MEDIATOR_STREAMING_GUIDE.md) with advanced streaming patterns and examples

### V1.2.0 - 12 July, 2025

-   Added automatic middleware discovery functionality for simplified configuration
-   Enhanced `AddMediator` method with `discoverMiddleware` parameter using method overloads
-   Automatic registration of all middleware implementations from specified assemblies
-   Support for middleware ordering through static/instance Order properties
-   Backward compatibility maintained with existing registration methods
-   Comprehensive documentation updates with auto-discovery examples

### V1.1.0 - 1 July, 2025

-   Enhanced middleware pipeline with conditional middleware support
-   Added `IMiddlewarePipelineInspector` for debugging and monitoring middleware execution
-   Full dependency injection support for middleware components
-   Performant middleware with conditional execution and optional priority-based execution
-   Enhanced pipeline inspection capabilities
-   Full test coverage with Shouldly assertions (replacing FluentAssertions)
-   Cleaned up samples and added middleware
-   Improved documentation with detailed examples and usage patterns

### V1.0.0 - 28 June, 2025

-   Initial release of Blazing.Mediator
-   Full CQRS support with separate Command and Query interfaces
-   Dependency injection integration with automatic handler registration
-   Multiple assembly scanning support
-   Comprehensive documentation and sample projects
-   .NET 9.0 support with nullable reference types
-   Extensive test coverage with unit and integration tests
