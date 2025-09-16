# TypedMiddlewareExample - Blazing.Mediator CQRS Type Constraints Demo

A demonstration project showcasing **type-constrained middleware** in Blazing.Mediator, highlighting the distinction between `ICommand` and `IQuery` interfaces with selective middleware application.

## ?? Overview

This example demonstrates how middleware can be selectively applied based on request types using generic type constraints, specifically showing how validation middleware can be applied only to commands while queries bypass validation entirely.

## ?? Key Features Demonstrated

### ?? CQRS Type Interfaces
- **Commands**: Use `ICommand` and `ICommand<TResponse>` interfaces for write operations
- **Queries**: Use `IQuery<TResponse>` interface for read operations
- **Type Safety**: Compile-time enforcement of CQRS patterns

### ?? Type-Constrained Middleware
- **Validation Middleware**: Constrained to `ICommand` interface only
- **Query Logging Middleware**: Constrained to `IQuery<TResponse>` interface
- **Selective Processing**: Middleware automatically applies only to appropriate request types

### ?? Clear Distinction Logging
- **Command Processing**: Shows "?? Processing ICommand" with validation middleware active
- **Query Processing**: Shows "?? Processing IQuery" with NO validation middleware
- **Visual Differentiation**: Easy identification of command vs query processing in logs

## ?? Project Structure

```
TypedMiddlewareExample/
??? Commands/           # ICommand and ICommand<T> implementations
?   ??? RegisterCustomerCommand.cs
?   ??? SendOrderConfirmationCommand.cs
?   ??? UpdateInventoryCommand.cs
?   ??? UpdateCustomerDetailsCommand.cs
??? Queries/           # IQuery<T> implementations
?   ??? GetProductQuery.cs
??? Handlers/          # Command and Query handlers
?   ??? RegisterCustomerCommandHandler.cs
?   ??? SendOrderConfirmationCommandHandler.cs
?   ??? UpdateInventoryCommandHandler.cs
?   ??? UpdateCustomerDetailsCommandHandler.cs
?   ??? GetProductQueryHandler.cs
??? Middleware/        # Type-constrained middleware
?   ??? ValidationMiddleware.cs (ICommand only)
?   ??? ErrorHandlingMiddleware.cs
?   ??? OperationalMonitoringMiddleware.cs
??? Validators/        # FluentValidation validators (commands only)
?   ??? RegisterCustomerCommandValidator.cs
?   ??? UpdateCustomerDetailsCommandValidator.cs
??? Services/          # Application services
?   ??? Runner.cs
?   ??? MiddlewarePipelineAnalyzer.cs
??? Logging/           # Custom console formatting
?   ??? SimpleConsoleFormatter.cs
?   ??? SimpleConsoleFormatterOptions.cs
??? Program.cs         # Application entry point
```

## ?? Middleware Pipeline

The middleware pipeline demonstrates selective execution based on request types:

### For Commands (`ICommand`, `ICommand<T>`):
1. `ErrorHandlingMiddleware` (int.MinValue) - Global error handling
2. `OperationalMonitoringMiddleware` (40) - Performance monitoring
3. `ValidationMiddleware` (100) - **Validation (Commands Only)**
4. **Handler** - Business logic execution

### For Queries (`IQuery<T>`):
1. `ErrorHandlingMiddleware` (int.MinValue) - Global error handling
2. `QueryLoggingMiddleware` (90) - **Query-specific logging**
3. `OperationalMonitoringMiddleware` (40) - Performance monitoring
4. **Handler** - Business logic execution (**No Validation**)

## ?? Type Constraints in Action

### Validation Middleware (Commands Only)
```csharp
// This middleware ONLY processes ICommand requests
public class ValidationMiddleware<TRequest> : IRequestMiddleware<TRequest>
    where TRequest : ICommand  // Type constraint ensures only commands are processed
{
    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        Logger.LogInformation("?? Processing ICommand: {RequestType} - Validation middleware active", typeof(TRequest).Name);
        await ValidateRequestAsync(request, cancellationToken);
        await next();
    }
}
```

### Query Logging Middleware (Queries Only)
```csharp
// This middleware ONLY processes IQuery requests
public class QueryLoggingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IQuery<TResponse>  // Type constraint ensures only queries are processed
{
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        Logger.LogInformation("?? Processing IQuery: {RequestType} - NO validation middleware", typeof(TRequest).Name);
        return await next();
    }
}
```

## ?? Running the Example

```bash
cd src/TypedMiddlewareExample
dotnet run
```

## ?? Expected Output

The application demonstrates the following workflow with clear type distinctions:

### 1. **?? Product Lookup (IQuery)**: 
- Shows query processing WITHOUT validation middleware
- Logs: "?? Processing IQuery: GetProductQuery - NO validation middleware"

### 2. **?? Inventory Management (ICommand<T>)**: 
- Shows command processing WITH validation middleware
- Logs: "?? Processing ICommand<TResponse>: UpdateInventoryCommand - Validation middleware active"

### 3. **?? Order Confirmation (ICommand)**: 
- Shows void command processing WITH validation middleware
- Logs: "?? Processing ICommand (void): SendOrderConfirmationCommand - Validation middleware active"

### 4. **?? Customer Registration (ICommand with Validation)**: 
- Demonstrates validation failure and success scenarios
- Shows validation middleware catching invalid data

### 5. **?? Customer Details Update (ICommand<T> with Validation)**: 
- Shows validation error handling and retry success
- Demonstrates commands that return values with validation

### Sample Output:
```
==============================================
Blazing.Mediator TypedMiddlewareExample
==============================================

Registered middleware:
  - [int.MinValue] ErrorHandlingMiddleware<TRequest>
  - [int.MinValue] ErrorHandlingMiddleware<TRequest, TResponse>
  - [40] OperationalMonitoringMiddleware<TRequest>
  - [40] OperationalMonitoringMiddleware<TRequest, TResponse>
  - [90] QueryLoggingMiddleware<TRequest, TResponse>
  - [100] ValidationMiddleware<TRequest>
  - [100] ValidationMiddleware<TRequest, TResponse>

-------- PRODUCT LOOKUP (IQuery) --------
info: ?? Processing IQuery: GetProductQuery - NO validation middleware (queries bypass validation)
dbug: >> Starting operation monitoring for: GetProductQuery
dbug: .. Retrieving product information for: WIDGET-001
dbug: << Operation monitoring completed for: GetProductQuery in 6ms
dbug: << Product found: -- Product: WIDGET-001 - High Quality Widget, Price: $99.99, In Stock: 25 units

-------- INVENTORY MANAGEMENT (ICommand<T>) --------
info: ?? Processing ICommand<TResponse>: UpdateInventoryCommand - Validation middleware active
dbug: >> Validating COMMAND request: UpdateInventoryCommand
dbug: -- No validators found for COMMAND UpdateInventoryCommand
dbug: >> Starting operation monitoring for: UpdateInventoryCommand
info: .. Updating inventory for product: WIDGET-001, change: -5
info: -- Inventory updated for WIDGET-001. New stock count: 20
dbug: << Operation monitoring completed for: UpdateInventoryCommand in 26ms
dbug: << New stock count: 20 units
```

## ?? Key Learning Points

This example teaches:

1. **Type-Constrained Middleware**: How to use generic type constraints to selectively apply middleware
2. **CQRS Interface Distinction**: Clear separation between command and query interfaces
3. **Selective Validation**: How validation can be applied only to commands, not queries
4. **Visual Logging**: Clear visual distinction between command and query processing
5. **Type Safety**: Compile-time enforcement of middleware applicability
6. **Performance Optimization**: Avoiding unnecessary middleware execution for inappropriate request types

## ??? Technologies Used

- **.NET 9.0**: Latest .NET framework with C# 13
- **Blazing.Mediator**: CQRS and mediator pattern implementation with type constraints
- **FluentValidation**: Declarative validation for commands only
- **Microsoft.Extensions.Hosting**: Application hosting and dependency injection
- **Custom Logging**: Clean console output formatting

## ?? Comparison with MiddlewareExample

| Feature | MiddlewareExample | TypedMiddlewareExample |
|---------|------------------|----------------------|
| Request Interfaces | `IRequest`, `IRequest<T>` | `ICommand`, `ICommand<T>`, `IQuery<T>` |
| Validation Scope | All requests | Commands only (type-constrained) |
| Middleware Targeting | Generic application | Type-specific application |
| Visual Distinction | Request names | Interface types (?? vs ??) |
| CQRS Clarity | Implicit | Explicit with type constraints |

This example builds upon the MiddlewareExample to show how type constraints can provide more precise control over middleware execution in CQRS applications.