# MiddlewareExample - Blazing.Mediator E-Commerce Demo

A comprehensive demonstration of the Blazing.Mediator library showcasing CQRS patterns, middleware pipelines, validation, and error handling through a realistic e-commerce scenario.

## 📋 Table of Contents

-   [🏗️ Architecture](#️-architecture)
-   [🔧 Design Principles](#-design-principles)
-   [🚀 E-Commerce Features Demonstrated](#-e-commerce-features-demonstrated)
    -   [📝 CQRS Implementation](#-cqrs-implementation)
    -   [🔄 Middleware Pipeline](#-middleware-pipeline)
    -   [✅ Validation](#-validation)
    -   [🛡️ Error Handling](#️-error-handling)
-   [🔍 E-Commerce Components Overview](#-e-commerce-components-overview)
    -   [Commands](#commands)
    -   [Queries](#queries)
    -   [Handlers](#handlers)
    -   [Middleware](#middleware)
    -   [Validators](#validators)
    -   [Services](#services)
    -   [Logging](#logging)
-   [🏃‍♂️ Running the Example](#️-running-the-example)
-   [📊 Expected Output](#-expected-output)
-   [🔗 Middleware Execution Order](#-middleware-execution-order)
-   [🔍 Middleware Pipeline Inspection](#-middleware-pipeline-inspection)
    -   [IMiddlewarePipelineInspector](#imiddlewarepipelineinspector)
    -   [MiddlewarePipelineAnalyzer Helper Class](#middlewarepipelineanalyzer-helper-class)
-   [📊 Mediator Statistics & Analysis](#-mediator-statistics--analysis)
    -   [MediatorStatistics Service](#mediatorstatistics-service)
    -   [Key Analysis Methods](#key-analysis-methods)
    -   [Analysis Output Features](#analysis-output-features)
    -   [Runtime Execution Statistics](#runtime-execution-statistics)
    -   [Integration with Application Flow](#integration-with-application-flow)
-   [🎯 Key Learnings](#-key-learnings)
-   [🔧 Technologies Used](#-technologies-used)
-   [📚 Further Reading](#-further-reading)

## 🏗️ Architecture

This project follows **CQRS** (Command Query Responsibility Segregation) principles and **Clean Architecture** patterns with a well-organized folder structure:

```
MiddlewareExample/
├── Commands/           # Write operations (UpdateInventory, SendConfirmation, RegisterCustomer, UpdateCustomerDetails)
├── Queries/           # Read operations (GetProduct)
├── Handlers/          # Business logic processors for each operation
├── Middleware/        # Cross-cutting concerns (Email, Caching, Audit, Monitoring, Error Handling, Validation)
├── Validators/        # Input validation logic (Customer registration and details update)
├── Services/          # Application services (Runner, MiddlewarePipelineAnalyzer)
├── Logging/           # Custom logging formatters for clean console output
└── Program.cs         # Application entry point with dependency injection setup
```

## 🔧 Design Principles

This example adheres to the following software engineering principles:

-   **KISS** (Keep It Simple, Stupid) - Simple, focused classes with single responsibilities
-   **DRY** (Don't Repeat Yourself) - Shared interfaces and base patterns
-   **SOLID** Principles:
    -   **S**ingle Responsibility: Each class and method has one reason to change
    -   **O**pen/Closed: Open for extension, closed for modification
    -   **L**iskov Substitution: Derived classes are substitutable for base classes
    -   **I**nterface Segregation: Small, focused interfaces
    -   **D**ependency Inversion: Depend on abstractions, not concretions
-   **YAGNI** (You Aren't Gonna Need It) - Only implements what's currently needed

## 🚀 E-Commerce Features Demonstrated

### ⚡ Powerful Auto-Registration

-   **Simple Setup**: Single-line mediator registration with automatic discovery of handlers and middleware
-   **Handler Auto-Discovery**: Automatically finds and registers all implementations
-   **Middleware Auto-Registration**: Seamlessly discovers and registers middleware components in proper execution order
-   **Statistics Tracking**: Optional statistics collection for runtime analysis of queries and commands
-   **Manual Override**: Optional manual registration available for advanced scenarios and fine-grained control
-   **Zero Configuration**: Works out-of-the-box with conventional patterns

### 📝 CQRS Implementation

-   **Commands**: Operations that modify state
    -   `UpdateInventoryCommand` - Stock level management
    -   `SendOrderConfirmationCommand` - Email notifications
    -   `RegisterCustomerCommand` - Customer onboarding with validation
    -   `UpdateCustomerDetailsCommand` - Customer information updates with validation and response handling
-   **Queries**: Operations that retrieve data
    -   `GetProductQuery` - Product information lookup with caching
-   **Handlers**: Separated business logic for each e-commerce operation

### 🔄 Middleware Pipeline

-   **ErrorHandlingMiddleware**: Global exception handling and error sanitization (supports both `<TRequest>` and `<TRequest, TResponse>`)
-   **ValidationMiddleware**: FluentValidation integration for customer operations (supports both request-only and request-response patterns)
-   **EmailLoggingMiddleware**: Email operation tracking and performance monitoring
-   **ProductQueryCacheMiddleware**: Product lookup caching simulation
-   **BusinessOperationAuditMiddleware**: Business operation auditing with timing
-   **OperationalMonitoringMiddleware**: Performance monitoring with detailed metrics
-   **Execution Order**: Demonstrates proper middleware ordering with custom order values

### ✅ Validation Middleware

-   **FluentValidation**: Declarative validation rules for customer operations
-   **Multi-Validator Support**: Multiple validators per command type
-   **Error Aggregation**: Collects and reports all validation errors
-   **Real-world Rules**:
    -   Customer name length validation
    -   Email format validation
    -   Customer ID format validation (CUST-XXXXXX pattern)
    -   Contact method enumeration validation
-   **Request-Response Pattern Support**: Validation for commands that return values

### 🛡️ Error Handling Middleware

-   **Global Exception Handling**: Catches and transforms exceptions
-   **Error Sanitization**: Prevents internal details from leaking to users
-   **Structured Logging**: Proper logging with correlation
-   **Validation Error Handling**: Specific handling for customer registration errors

## 🔍 E-Commerce Components Overview

### Commands

-   `UpdateInventoryCommand.cs` - Stock level management for products
-   `SendOrderConfirmationCommand.cs` - Order confirmation email sending
-   `RegisterCustomerCommand.cs` - Customer registration with validation
-   `UpdateCustomerDetailsCommand.cs` - Customer information updates with validation and boolean response

### Queries

-   `GetProductQuery.cs` - Product information retrieval with caching

### Handlers

-   `UpdateInventoryCommandHandler.cs` - Inventory management business logic
-   `SendOrderConfirmationCommandHandler.cs` - Email confirmation processing
-   `RegisterCustomerCommandHandler.cs` - Customer registration business logic
-   `UpdateCustomerDetailsCommandHandler.cs` - Customer details update processing
-   `GetProductQueryHandler.cs` - Product lookup processing

### Middleware

-   `ErrorHandlingMiddleware.cs` - Global error handling (Order: int.MinValue)
-   `ValidationMiddleware.cs` - Input validation (Order: 100)
-   `EmailLoggingMiddleware.cs` - Email operation logging (Order: 10)
-   `ProductQueryCacheMiddleware.cs` - Product caching simulation (Order: 20)
-   `BusinessOperationAuditMiddleware.cs` - Business operation auditing (Order: 30)
-   `OperationalMonitoringMiddleware.cs` - Performance monitoring (Order: 40)

### Validators

-   `RegisterCustomerCommandValidator.cs` - FluentValidation rules for customer registration
-   `UpdateCustomerDetailsCommandValidator.cs` - FluentValidation rules for customer details updates

### Services

-   `Runner.cs` - E-commerce demonstration service:
    -   `InspectMiddlewarePipeline()` - Pipeline inspection and diagnostics using `IMiddlewarePipelineInspector`
    -   `InspectMediatorTypes()` - Comprehensive analysis of registered queries and commands using `MediatorStatistics`
    -   `DemonstrateProductLookup()` - Product query functionality
    -   `DemonstrateInventoryManagement()` - Stock management
    -   `DemonstrateOrderConfirmation()` - Email notifications
    -   `DemonstrateCustomerRegistration()` - Customer onboarding with validation
    -   `DemonstrateCustomerDetailsUpdate()` - Customer details update with validation error handling and retry success
    -   Constructor parameters: `IMediator`, `ILogger<Runner>`, `IMiddlewarePipelineInspector`, `IServiceProvider`, and `MediatorStatistics`
-   `MiddlewarePipelineAnalyzer.cs` - Advanced static helper class for middleware pipeline analysis:
    -   `AnalyzeMiddleware()` - Processes and sorts middleware by execution order using `IMiddlewarePipelineInspector`
    -   `ExtractMiddlewareInfo()` - Extracts detailed information from middleware types and actual order values
    -   `ExtractClassNameAndTypeParameters()` - Handles both generic and concrete type parameter extraction
    -   `FormatOrderValue()` - Formats special order values (int.MinValue/MaxValue) for display
    -   `MiddlewareInfo` record - Immutable data structure containing order, display format, class name, and type parameters

### Logging

-   `SimpleConsoleFormatter.cs` - Custom console formatter for clean, readable output
-   `SimpleConsoleFormatterOptions.cs` - Configuration options for the custom formatter

## 🏃‍♂️ Running the Example

```bash
cd src/MiddlewareExample
dotnet run
```

### ⚙️ Configuration

The example is configured with the following settings in `Program.cs`:

```csharp
services.AddMediator(
    enableStatisticsTracking: true,    // Enable runtime statistics collection
    discoverMiddleware: true,          // Automatically discover middleware
    assemblies: Assembly.GetExecutingAssembly());
```

**Key Configuration Parameters:**
- `enableStatisticsTracking: true` - Enables collection of execution statistics and runtime analysis
- `discoverMiddleware: true` - Automatically discovers and registers middleware from the assembly
- `assemblies` - Specifies which assemblies to scan for handlers and middleware

## 📊 Expected Output

The application demonstrates the following e-commerce workflow:

1. **🔍 Product Lookup**: Product information retrieval with caching middleware
2. **📦 Inventory Management**: Stock level updates with business operation auditing
3. **📧 Order Confirmation**: Email notification sending with operational monitoring
4. **👤 Customer Registration**: Customer onboarding with validation error handling
5. **🔄 Customer Details Update**: Customer information updates with validation, error handling, and retry success

### Sample Output:

```
==============================================
Blazing.Mediator Simple Example
==============================================

This project demonstrates the core features of Blazing.Mediator.

It includes a simple e-commerce scenario with product lookup,
inventory management, order confirmation, and customer registration.

The example showcases how to use Blazing.Mediator with middleware
for error handling, validation, and logging.

The code is structured to demonstrate best practices for building
scalable and maintainable applications using Blazing.Mediator.

==============================================

Registered middleware:
  - [int.MinValue] ErrorHandlingMiddleware<TRequest>
  - [int.MinValue] ErrorHandlingMiddleware<TRequest, TResponse>
  - [10] EmailLoggingMiddleware<SendOrderConfirmationCommand>
  - [20] ProductQueryCacheMiddleware<GetProductQuery, String>
  - [30] BusinessOperationAuditMiddleware<TRequest, TResponse>
  - [40] OperationalMonitoringMiddleware<TRequest>
  - [100] ValidationMiddleware<TRequest>
  - [100] ValidationMiddleware<TRequest, TResponse>

=== COMPREHENSIVE MEDIATOR ANALYSIS ===

COMPACT MODE (isDetailed: false):
══════════════════════════════════════
* 1 QUERIES DISCOVERED:
  * Assembly: MiddlewareExample
    * Namespace: MiddlewareExample.Queries
      + GetProductQuery : IRequest<String>

* 4 COMMANDS DISCOVERED:
  * Assembly: MiddlewareExample
    * Namespace: MiddlewareExample.Commands
      + UpdateInventoryCommand : IRequest<Int32>
      + SendOrderConfirmationCommand : IRequest
      + RegisterCustomerCommand : IRequest
      + UpdateCustomerDetailsCommand : IRequest<Boolean>

LEGEND: + = Handler found, ! = No handler, # = Multiple handlers

DETAILED MODE (isDetailed: true - Default):
════════════════════════════════════════════════
* 1 QUERIES DISCOVERED:
  * Assembly: MiddlewareExample
    * Namespace: MiddlewareExample.Queries
      + GetProductQuery : IRequest<String>
        │ Type:        MiddlewareExample.Queries.GetProductQuery
        │ Returns:     String
        │ Handler:     GetProductQueryHandler
        │ Status:      Single
        │ Assembly:    MiddlewareExample
        │ Namespace:   MiddlewareExample.Queries
        │ Handler(s):  1 registered
        └─ Result Type: NO (standard type)

* 4 COMMANDS DISCOVERED:
  * Assembly: MiddlewareExample
    * Namespace: MiddlewareExample.Commands
      + UpdateInventoryCommand : IRequest<Int32>
        │ Type:        MiddlewareExample.Commands.UpdateInventoryCommand
        │ Returns:     Int32
        │ Handler:     UpdateInventoryCommandHandler
        │ Status:      Single
        │ Assembly:    MiddlewareExample
        │ Namespace:   MiddlewareExample.Commands
        │ Handler(s):  1 registered
        └─ Result Type: NO (standard type)

      + SendOrderConfirmationCommand : IRequest
        │ Type:        MiddlewareExample.Commands.SendOrderConfirmationCommand
        │ Returns:     void
        │ Handler:     SendOrderConfirmationCommandHandler
        │ Status:      Single
        │ Assembly:    MiddlewareExample
        │ Namespace:   MiddlewareExample.Commands
        │ Handler(s):  1 registered
        └─ Result Type: NO (standard type)

      + RegisterCustomerCommand : IRequest
        │ Type:        MiddlewareExample.Commands.RegisterCustomerCommand
        │ Returns:     void
        │ Handler:     RegisterCustomerCommandHandler
        │ Status:      Single
        │ Assembly:    MiddlewareExample
        │ Namespace:   MiddlewareExample.Commands
        │ Handler(s):  1 registered
        └─ Result Type: NO (standard type)

      + UpdateCustomerDetailsCommand : IRequest<Boolean>
        │ Type:        MiddlewareExample.Commands.UpdateCustomerDetailsCommand
        │ Returns:     Boolean
        │ Handler:     UpdateCustomerDetailsCommandHandler
        │ Status:      Single
        │ Assembly:    MiddlewareExample
        │ Namespace:   MiddlewareExample.Commands
        │ Handler(s):  1 registered
        └─ Result Type: NO (standard type)

LEGEND:
  + = Handler found (Single)    ! = No handler (Missing)    # = Multiple handlers
  │ = Property details          └─ = Additional information
===============================================

info: Starting E-Commerce Demo with Blazing.Mediator...

-------- PRODUCT LOOKUP --------
dbug: >> Looking up product: WIDGET-001
dbug: >> Checking cache for product: WIDGET-001
dbug: >> Starting business operation audit: GetProductQuery
dbug: .. Retrieving product information for: WIDGET-001
info: << Business operation completed: GetProductQuery in 5ms
dbug: << Product found: -- Product: WIDGET-001 - High Quality Widget, Price: $99.99, In Stock: 25 units

-------- INVENTORY MANAGEMENT --------
dbug: >> Updating inventory for: WIDGET-001, change: -5
dbug: .. Updating inventory for product: WIDGET-001, change: -5
info: -- Inventory updated for WIDGET-001. New stock count: 20
dbug: << New stock count: 20 units

-------- ORDER CONFIRMATION --------
dbug: >> Sending order confirmation for: ORD-2025-001 to: customer@example.com
dbug: >> Email operation started for order: ORD-2025-001 to: customer@example.com
dbug: .. Sending order confirmation for order: ORD-2025-001 to: customer@example.com
info: -- Order confirmation email sent successfully for order ORD-2025-001
dbug: << Order confirmation sent successfully!

-------- CUSTOMER REGISTRATION (With Validation) --------
dbug: >> Registering customer: J (john.doe@example.com)
warn: !! Validation failed for RegisterCustomerCommand: Full name must be at least 2 characters
fail: !! Customer registration failed due to validation errors: Validation failed while processing the request

-------- CUSTOMER DETAILS UPDATE (With Validation Error & Retry Success) --------
dbug: >> Updating customer details (invalid data): INVALID-ID - John Doe (john.doe@example.com)
warn: !! Validation failed for UpdateCustomerDetailsCommand: Customer ID must be in format CUST-XXXXXX
fail: !! Customer details update failed due to validation errors: Validation failed while processing the request

dbug: >> Updating customer details (valid data): CUST-123456 - John Doe (john.doe@example.com)
info: .. Updating customer details for ID: CUST-123456
info: -- Customer details updated successfully for CUST-123456
dbug: << Customer details updated successfully!

=== EXECUTION STATISTICS ===
Query Executions:
  GetProductQuery: 1 execution(s)

Command Executions:
  UpdateInventoryCommand: 1 execution(s)
  SendOrderConfirmationCommand: 1 execution(s)
  RegisterCustomerCommand: 1 execution(s)
  UpdateCustomerDetailsCommand: 2 execution(s)

Total Operations: 6
=============================

info: E-Commerce Demo completed!
```

## 🔗 Middleware Execution Order

The middleware pipeline executes in the following order for e-commerce operations:

1. `ErrorHandlingMiddleware` (int.MinValue) - Wraps everything in try-catch for both request-only and request-response patterns
2. `EmailLoggingMiddleware` (10) - Email operation logging (specific to SendOrderConfirmationCommand)
3. `ProductQueryCacheMiddleware` (20) - Product caching simulation (specific to GetProductQuery)
4. `BusinessOperationAuditMiddleware` (30) - Business operation tracking
5. `OperationalMonitoringMiddleware` (40) - Performance monitoring
6. `ValidationMiddleware` (100) - Input validation for commands with validators (supports both patterns)
7. **Handler** - Business logic execution

**Note**: The middleware registration shows both generic (`<TRequest>`, `<TRequest, TResponse>`) and specific type implementations, demonstrating how Blazing.Mediator handles different request patterns efficiently.

## 🔍 Middleware Pipeline Inspection

This example demonstrates advanced middleware pipeline analysis capabilities using Blazing.Mediator's inspection features:

### IMiddlewarePipelineInspector

The `IMiddlewarePipelineInspector` interface provides runtime inspection of the registered middleware pipeline:

```csharp
public class Runner(
    IMediator mediator,
    ILogger<Runner> logger,
    IMiddlewarePipelineInspector pipelineInspector,  // Injected pipeline inspector
    IServiceProvider serviceProvider)
{
    public void InspectMiddlewarePipeline()
    {
        var middlewareAnalysis = MiddlewarePipelineAnalyzer.AnalyzeMiddleware(pipelineInspector, serviceProvider);

        Console.WriteLine("Registered middleware:");
        foreach (var middleware in middlewareAnalysis)
        {
            Console.WriteLine($"  - [{middleware.OrderDisplay}] {middleware.ClassName}{middleware.TypeParameters}");
        }
    }
}
```

### MiddlewarePipelineAnalyzer Helper Class

The `MiddlewarePipelineAnalyzer` is a useful helper that processes middleware registration information:

**Key Features:**

-   **Order Extraction**: Retrieves actual execution order values from dependency injection containers
-   **Type Analysis**: Extracts generic type parameters from both generic and concrete middleware implementations
-   **Smart Formatting**: Converts special order values (`int.MinValue`, `int.MaxValue`) to readable strings
-   **Sorting**: Orders middleware by execution precedence for clear visualization

**Core Methods:**

-   `AnalyzeMiddleware()`: Main entry point that processes all registered middleware
-   `ExtractMiddlewareInfo()`: Extracts detailed information from middleware types
-   `ExtractClassNameAndTypeParameters()`: Handles both generic (`<TRequest>`) and concrete type extraction
-   `FormatOrderValue()`: Formats order values for display

**MiddlewareInfo Record:**

```csharp
public record MiddlewareInfo(
    int Order,              // Numeric execution order
    string OrderDisplay,    // Formatted order (e.g., "int.MinValue", "100")
    string ClassName,       // Clean class name without generic suffixes
    string TypeParameters   // Generic type parameters (e.g., "<TRequest, TResponse>")
);
```

**Example Output:**

```
Registered middleware:
  - [int.MinValue] ErrorHandlingMiddleware<TRequest>
  - [int.MinValue] ErrorHandlingMiddleware<TRequest, TResponse>
  - [10] EmailLoggingMiddleware<SendOrderConfirmationCommand>
  - [20] ProductQueryCacheMiddleware<GetProductQuery, String>
  - [30] BusinessOperationAuditMiddleware<TRequest, TResponse>
  - [40] OperationalMonitoringMiddleware<TRequest>
  - [100] ValidationMiddleware<TRequest>
  - [100] ValidationMiddleware<TRequest, TResponse>
```

This inspection capability is invaluable for:

-   **Debugging**: Understanding middleware execution order during development
-   **Documentation**: Automatically generating pipeline documentation
-   **Testing**: Verifying correct middleware registration in unit tests
-   **Monitoring**: Runtime analysis of pipeline configuration

## 📊 Mediator Statistics & Analysis

This example demonstrates comprehensive mediator analysis and statistics tracking capabilities using Blazing.Mediator's built-in statistics features:

### MediatorStatistics Service

The `MediatorStatistics` service provides runtime analysis and execution tracking:

```csharp
public class Runner(
    IMediator mediator,
    ILogger<Runner> logger,
    IMiddlewarePipelineInspector pipelineInspector,
    IServiceProvider serviceProvider,
    MediatorStatistics mediatorStatistics)  // Injected statistics service
{
    // ... implementation
}
```

### Key Analysis Methods

**Query and Command Analysis:**

-   `AnalyzeQueries(serviceProvider, isDetailed)`: Analyzes all registered query types with handler information
-   `AnalyzeCommands(serviceProvider, isDetailed)`: Analyzes all registered command types with handler information
-   `ReportStatistics()`: Reports execution statistics collected during runtime

**Analysis Modes:**

-   **Compact Mode** (`isDetailed: false`): Shows essential information with status indicators
-   **Detailed Mode** (`isDetailed: true`, default): Shows comprehensive information including types, handlers, and response details

**Handler Status Indicators:**

-   `+` = Single handler found (Correct)
-   `!` = No handler registered (Missing)
-   `#` = Multiple handlers registered (Conflict)

### Analysis Output Features

**Compact Analysis Example:**

```
* 1 QUERIES DISCOVERED:
  * Assembly: MiddlewareExample
    * Namespace: MiddlewareExample.Queries
      + GetProductQuery : IRequest<String>

* 4 COMMANDS DISCOVERED:
  * Assembly: MiddlewareExample
    * Namespace: MiddlewareExample.Commands
      + UpdateInventoryCommand : IRequest<Int32>
      + SendOrderConfirmationCommand : IRequest
      + RegisterCustomerCommand : IRequest
      + UpdateCustomerDetailsCommand : IRequest<Boolean>

LEGEND: + = Handler found, ! = No handler, # = Multiple handlers
```

**Detailed Analysis Example:**

```
* 1 QUERIES DISCOVERED:
  * Assembly: MiddlewareExample
    * Namespace: MiddlewareExample.Queries
      + GetProductQuery : IRequest<String>
        │ Type:        MiddlewareExample.Queries.GetProductQuery
        │ Returns:     String
        │ Handler:     GetProductQueryHandler
        │ Status:      Single
        │ Assembly:    MiddlewareExample
        │ Namespace:   MiddlewareExample.Queries
        │ Handler(s):  1 registered
        └─ Result Type: NO (standard type)
```

### Runtime Execution Statistics

The statistics service tracks actual command/query execution counts during runtime:

```
=== EXECUTION STATISTICS ===
Query Executions:
  GetProductQuery: 1 execution(s)

Command Executions:
  UpdateInventoryCommand: 1 execution(s)
  SendOrderConfirmationCommand: 1 execution(s)
  RegisterCustomerCommand: 1 execution(s)
  UpdateCustomerDetailsCommand: 2 execution(s)

Total Operations: 6
=============================
```

### Integration with Application Flow

The example demonstrates statistics integration at multiple points:

1. **Startup Analysis**: Comprehensive application structure analysis before execution
2. **Runtime Tracking**: Automatic collection of execution statistics during operations
3. **Final Report**: Summary of all operations performed during the session

This provides valuable insights for:

-   **Development**: Understanding application structure and handler coverage
-   **Testing**: Verifying all commands/queries have proper handlers
-   **Monitoring**: Tracking operation frequency and patterns
-   **Debugging**: Identifying missing or duplicate handlers
-   **Documentation**: Automatically generating application maps

## 🎯 Key Learnings

This example teaches:

-   **Auto-Registration Power**: How Blazing.Mediator's automatic discovery eliminates boilerplate dependency injection code
-   **Convention over Configuration**: Working with established patterns for seamless handler and middleware registration
-   **Statistics Tracking**: Comprehensive runtime analysis and execution tracking for queries and commands
-   **Application Analysis**: Automatic discovery and validation of handler registration across your application
-   **CQRS Pattern Implementation**: Proper separation of read/write operations in e-commerce context
-   **SOLID Principles**: Single responsibility methods in the Runner service
-   **Middleware Pipeline Design**: Cross-cutting concerns and execution order with custom ordering
-   **Validation Strategy**: Declarative validation with error aggregation for customer data
-   **Error Handling**: Global exception handling with error sanitization for both request-only and request-response patterns
-   **Dependency Injection**: Proper service registration and resolution with enhanced statistics capabilities
-   **Clean Architecture**: Organized folder structure and separation of concerns
-   **Real-world Scenarios**: Product management, inventory control, email notifications, customer onboarding, and data updates
-   **Request-Response Patterns**: Handling commands that return values (like `UpdateCustomerDetailsCommand` returning `bool`)
-   **Middleware Specificity**: How middleware can be applied to specific command types or generically
-   **Pipeline Analysis**: Runtime inspection of middleware registration and execution order using `IMiddlewarePipelineInspector`
-   **Mediator Statistics**: Comprehensive analysis of commands, queries, handlers, and execution patterns
-   **Advanced Reflection**: Type analysis and generic parameter extraction for middleware introspection
-   **Structured Logging**: Clean console output with custom formatters for better readability
-   **Helper Utilities**: Building reusable analyzer classes for framework introspection (`MiddlewarePipelineAnalyzer`)
-   **Runtime Monitoring**: Tracking execution frequency and patterns for operational insights

## 🔧 Technologies Used

-   **.NET 9.0**: Latest .NET framework
-   **Blazing.Mediator**: CQRS and mediator pattern implementation
-   **FluentValidation**: Declarative validation library with dependency injection extensions
-   **Microsoft.Extensions.Hosting**: Application hosting and dependency injection
-   **Microsoft.Extensions.Logging**: Structured logging with custom console formatters

## 📚 Further Reading

-   [Blazing.Mediator Documentation](../../docs/)
-   [CQRS Pattern Guide](../../docs/MEDIATOR_PATTERN_GUIDE.md)
-   [Notification Guide](../../docs/NOTIFICATION_GUIDE.md)
