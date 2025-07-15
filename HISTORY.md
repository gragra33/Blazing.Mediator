# Version History

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
