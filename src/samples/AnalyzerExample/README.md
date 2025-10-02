# Blazing.Mediator Multi-Assembly Analyzer Example

This comprehensive example demonstrates all the analysis capabilities of Blazing.Mediator across multiple assemblies, showcasing how to inspect and understand your application's mediator usage patterns with advanced type formatting capabilities in a real-world multi-project architecture.

## **Demonstrated Example Benefits for Debugging with Analyzers**

**This AnalyzerExample project intentionally includes missing handlers to demonstrate the powerful benefits of Blazing.Mediator's analysis capabilities in debugging real-world scenarios.** When you have many Queries, Commands, Notifications, Request Middleware, and Notification Middleware across multiple projects, the Analyzers make it extremely easy to diagnose issues, identify missing components, and understand your application's mediator usage patterns.

**The AnalyzerExample sample application highlights queries, commands, and notifications that have missing handlers in bright red text**, making it immediately obvious which components need to be implemented during development and debugging.

### **Missing Handlers by Design**
This example includes **intentionally missing handlers** across all projects to showcase how the Analyzers help you:
- **Quickly identify missing handlers** that would cause runtime errors
- **Discover unmatched requests** in complex multi-project solutions  
- **Validate handler registration** across assembly boundaries
- **Understand interface implementations** and handler patterns
- **Debug middleware configuration** and execution order
- **Visual identification** with red console highlighting for missing components

### **Scale of the Multi-Assembly Example**
This comprehensive example includes **92 total mediator components** across 5 projects:

| Component Type | AnalyzerExample.Common | AnalyzerExample.Products | AnalyzerExample.Users | AnalyzerExample.Orders | Main Assembly | **Total** |
|----------------|:----------------------:|:------------------------:|:---------------------:|:----------------------:|:-------------:|:---------:|
| **Queries** | 1 | 6 | 7 | 7 | 0 | **21** |
| **Commands** | 2 | 10 | 8 | 8 | 0 | **28** |
| **Notifications** | 2 | 9 | 8 | 6 | 0 | **25** |
| **Request Middleware** | 4 | 4 | 0 | 0 | 0 | **8** |
| **Notification Middleware** | 2 | 0 | 0 | 0 | 0 | **2** |
| **Handlers** | 4 | 21 | 13 | 13 | 0 | **51** |
| **TOTAL COMPONENTS** | **15** | **50** | **36** | **34** | **0** | **135** |

**Note**: Some handlers are intentionally missing to demonstrate analyzer benefits - see the missing handler analysis in the output.

With this many components across multiple projects, **manually tracking missing handlers would be nearly impossible**. The Blazing.Mediator Analyzers make it trivial to:

* **Instantly identify all missing handlers** across the entire solution  
* **Validate multi-project handler registration** with single commands  
* **Understand complex domain boundaries** and interface implementations  
* **Debug middleware pipeline configuration** across assemblies  
* **Monitor handler status changes** during development  
* **Ensure complete test coverage** by identifying untested components

## What This Example Demonstrates

This multi-assembly application provides complete examples of:

1. **Cross-Assembly Query Analysis** - `AnalyzeQueries()` with domain-specific patterns across projects
2. **Cross-Assembly Command Analysis** - `AnalyzeCommands()` with business operations across modules  
3. **Cross-Assembly Notification Analysis** - `AnalyzeNotifications()` with domain events and integration events
4. **Multi-Project Request Middleware Analysis** - `MiddlewarePipelineBuilder.AnalyzeMiddleware()` with shared and domain-specific middleware
5. **Multi-Project Notification Middleware Analysis** - `NotificationPipelineBuilder.AnalyzeMiddleware()` with comprehensive cross-assembly formatting
6. **Advanced Cross-Assembly Type Formatting** - Extension methods showcasing clean type names across different assemblies and namespaces
7. **Missing Handler Detection** - Comprehensive analysis showing which handlers are missing across all projects

## Multi-Assembly Project Structure

```
AnalyzerExample Solution/
├── AnalyzerExample/                     # Main coordination project
│   ├── Services/
│   │   └── AnalysisService.cs          # Multi-assembly analysis orchestration
│   └── Program.cs                      # Cross-assembly discovery and configuration
│
├── AnalyzerExample.Common/              # Shared infrastructure
│   ├── Domain/
│   │   └── CommonModels.cs             # Base entities, pagination, operation results
│   ├── Interfaces/
│   │   └── CommonInterfaces.cs         # Shared contracts (IAuditableCommand, IDomainEvent, etc.)
│   └── Middleware/
│       └── CommonMiddleware.cs         # Global middleware (logging, audit, transaction)
│
├── AnalyzerExample.Products/            # Product domain module
│   ├── Domain/ProductModels.cs         # Product, ProductReview, Category entities
│   ├── Queries/ProductQueries.cs       # Product search, filtering, stats queries
│   ├── Commands/ProductCommands.cs     # CRUD, stock management, import commands
│   ├── Notifications/ProductNotifications.cs # Product domain events
│   ├── Handlers/ProductHandlers.cs     # Product business logic
│   └── Middleware/ProductMiddleware.cs  # Product-specific middleware
│
├── AnalyzerExample.Users/               # User management module
│   ├── Domain/UserModels.cs            # User, UserProfile, UserRole entities
│   ├── Queries/UserQueries.cs          # User search, stats, role queries
│   ├── Commands/UserCommands.cs        # User lifecycle, role assignment commands
│   ├── Notifications/UserNotifications.cs # User domain events
│   └── Handlers/UserHandlers.cs        # User management logic
│
└── AnalyzerExample.Orders/              # Order processing module
    ├── Domain/OrderModels.cs           # Order, OrderItem, Address entities
    ├── Queries/OrderQueries.cs         # Order search, tracking, stats queries
    ├── Commands/OrderCommands.cs       # Order lifecycle, fulfillment commands
    ├── Notifications/OrderNotifications.cs # Order domain events
    └── Handlers/OrderHandlers.cs       # Order processing logic
```

## Cross-Assembly Analysis Examples

### Domain-Specific Query Patterns
- **Product Queries**: Inventory search, category filtering, review aggregation
- **User Queries**: User management, role-based filtering, activity tracking
- **Order Queries**: Order tracking, status filtering, customer order history
- **Cross-Domain Queries**: Complex queries spanning multiple domain boundaries

### Business Command Patterns  
- **Product Commands**: Inventory management, pricing, review processing
- **User Commands**: Registration, profile management, role assignments
- **Order Commands**: Order creation, fulfillment, status updates
- **Transaction Commands**: Multi-domain operations with distributed transactions

### Domain Event Patterns
- **Product Events**: Stock changes, price updates, reviews added
- **User Events**: Registration, profile updates, login tracking
- **Order Events**: Status changes, shipment tracking, delivery confirmation
- **Integration Events**: Cross-domain communication and external system integration

### Multi-Project Middleware Architecture

#### Shared Common Middleware
- **Global Logging Middleware**: Request/response logging across all assemblies
- **Audit Middleware**: Business operation auditing with user tracking
- **Transaction Middleware**: Distributed transaction coordination
- **Domain Event Middleware**: Cross-assembly domain event processing

#### Domain-Specific Middleware
- **Product Middleware**: Product validation, inventory checks, caching strategies
- **User Middleware**: Authentication, authorization, profile validation
- **Order Middleware**: Order validation, payment processing, fulfillment workflows

## Advanced Cross-Assembly Type Normalization

The example demonstrates comprehensive extension methods providing clean type formatting across multiple assemblies:

### Cross-Assembly Query/Command Normalization
```csharp
// From AnalyzerExample.Products assembly
var productQuery = analysis.FirstOrDefault(q => q.Assembly == "AnalyzerExample.Products");

// Before: Raw .NET generic types with backticks
var rawResponseType = productQuery.ResponseType; // "PagedResult`1[ProductSummaryDto]"
var rawInterface = productQuery.PrimaryInterface; // "IProductQuery<PagedResult`1>"

// After: Clean cross-assembly normalization  
var normalizedResponse = productQuery.NormalizeResponseTypeName(); // "PagedResult<ProductSummaryDto>"
var normalizedInterface = productQuery.NormalizePrimaryInterfaceName(); // "IProductQuery<PagedResult<ProductSummaryDto>>"
var assemblyInfo = $"{productQuery.Namespace}, {productQuery.Assembly}"; // "AnalyzerExample.Products.Queries, AnalyzerExample.Products"
```

### Cross-Assembly Middleware Normalization
```csharp
// From AnalyzerExample.Common assembly  
var globalMiddleware = middlewareAnalysis.FirstOrDefault(m => m.GetAssemblyName() == "AnalyzerExample.Common");

// Before: Raw middleware types with backticks
var rawMiddlewareType = globalMiddleware.Type; // "GlobalLoggingMiddleware`1[TRequest]"

// After: Comprehensive cross-assembly normalization
var normalizedType = globalMiddleware.NormalizeTypeName(); // "GlobalLoggingMiddleware<TRequest>"
var assemblyName = globalMiddleware.GetAssemblyName(); // "AnalyzerExample.Common"
var namespaceName = globalMiddleware.GetNamespace(); // "AnalyzerExample.Common.Middleware"
var summary = globalMiddleware.NormalizeSummary(); // "[1000] GlobalLoggingMiddleware<TRequest>"
var fullSummary = globalMiddleware.NormalizeSummary(includeNamespace: true); 
// "[1000] GlobalLoggingMiddleware<TRequest> (AnalyzerExample.Common.Middleware, AnalyzerExample.Common)"
```

### Assembly and Namespace Analysis
```csharp
// Cross-assembly distribution analysis
var assembliesByQueries = queries.GroupBy(q => q.Assembly).Count(); // 4 assemblies
var namespacesByCommands = commands.GroupBy(c => c.Namespace).Count(); // 12 namespaces
var middlewareDistribution = middleware.GroupBy(m => m.GetAssemblyName())
    .ToDictionary(g => g.Key, g => g.Count()); // Distribution across assemblies
```

## Running the Multi-Assembly Example

1. **Navigate to the main project directory:**
   ```bash
   cd src/samples/AnalyzerExample
   ```

2. **Build the solution (builds all referenced projects):**
   ```bash
   dotnet build
   ```

3. **Run the multi-assembly analysis:**
   ```bash
   dotnet run
   ```

4. **View comprehensive cross-assembly analysis** showing:
   - Queries, commands, and notifications organized by assembly and namespace
   - Handler discovery across multiple projects with clean names
   - Middleware pipeline analysis showing cross-project dependencies
   - Type normalization comparisons across assemblies
   - Assembly distribution statistics and insights

## Sample Multi-Assembly Output

```
BLAZING.MEDIATOR MULTI-ASSEMBLY ANALYZER EXAMPLE
====================================================

This example demonstrates comprehensive analysis capabilities across multiple assemblies:
  - AnalyzerExample.Common - Shared middleware and interfaces
  - AnalyzerExample.Products - Product domain with queries, commands, and handlers  
  - AnalyzerExample.Users - User management with complex domain models
  - AnalyzerExample.Orders - Order processing with status tracking
  - Main Assembly - Coordination and cross-cutting concerns

CROSS-ASSEMBLY QUERY ANALYSIS
==================================

[INFO] Found 21 query types across 4 assemblies:

Assembly: AnalyzerExample.Products
  Namespace: AnalyzerExample.Products.Queries
    + GetProductsQuery : IProductQuery<PagedResult<ProductSummaryDto>>
    ! GetLowStockProductsQuery : IRequest<List<ProductDto>>  <- MISSING HANDLER
    + GetProductByIdQuery : IRequest<ProductDetailDto>
       [NORMALIZED] Response: PagedResult<ProductSummaryDto>
       [NORMALIZED] Interface: IProductQuery<PagedResult<ProductSummaryDto>>

Assembly: AnalyzerExample.Users
  Namespace: AnalyzerExample.Users.Queries
    + GetUsersQuery : IUserQuery<PagedResult<UserSummaryDto>>
    ! GetInactiveUsersQuery : IRequest<List<UserDto>>  <- MISSING HANDLER
       [NORMALIZED] Response: PagedResult<UserSummaryDto>

CROSS-ASSEMBLY COMMAND ANALYSIS
==================================

[INFO] Found 28 command types across 4 assemblies:
   ! = Missing Handlers: 8 commands without handlers detected

MULTI-PROJECT REQUEST MIDDLEWARE ANALYSIS
=============================================

[INFO] Found 8 request middleware across 2 assemblies:

Assembly: AnalyzerExample.Common
  Namespace: AnalyzerExample.Common.Middleware
    - Middleware: GlobalLoggingMiddleware
       Order: 1000
       [NORMALIZED] Type: GlobalLoggingMiddleware<TRequest>
       [NORMALIZED] Type Name Only: GlobalLoggingMiddleware<TRequest>
       [NORMALIZED] Summary: [1000] GlobalLoggingMiddleware<TRequest>
       [NORMALIZED] Full Summary: [1000] GlobalLoggingMiddleware<TRequest> (AnalyzerExample.Common.Middleware, AnalyzerExample.Common)

Assembly: AnalyzerExample.Products  
  Namespace: AnalyzerExample.Products.Middleware
    - Middleware: ProductValidationMiddleware
       Order: 150
       [NORMALIZED] Type: ProductValidationMiddleware<TRequest>
       [NORMALIZED] Type Name Only: ProductValidationMiddleware<TRequest>
       [NORMALIZED] Summary: [150] ProductValidationMiddleware<TRequest>

[CROSS-ASSEMBLY DISTRIBUTION]:
   Queries across 4 assemblies
   Commands across 4 assemblies  
   Notifications across 4 assemblies
   Request middleware across 2 assemblies
   Notification middleware across 1 assemblies

[MISSING HANDLERS SUMMARY]:
   Missing Query Handlers: 3 queries need handlers
   Missing Command Handlers: 8 commands without handlers detected  
   Missing Notification Handlers: 2 notifications need handlers
   Total Missing: 13 handlers need to be implemented
```

## Multi-Assembly Configuration

The example shows how to configure mediator analysis across multiple assemblies:

### Manual Configuration
```csharp
services.AddMediator(config =>
{
    // Enable comprehensive statistics tracking across all assemblies
    config.WithStatisticsTracking();
    
    // Register common middleware that works across all assemblies
    config.AddMiddleware(typeof(AnalyzerExample.Common.Middleware.GlobalLoggingMiddleware<>));
    config.AddMiddleware(typeof(AnalyzerExample.Common.Middleware.GlobalLoggingMiddleware<,>));
    config.AddMiddleware(typeof(AnalyzerExample.Common.Middleware.AuditMiddleware<>));
    config.AddMiddleware(typeof(AnalyzerExample.Common.Middleware.TransactionMiddleware<>));
    
    // Register domain-specific middleware
    config.AddMiddleware(typeof(AnalyzerExample.Products.Middleware.ProductValidationMiddleware<>));
    config.AddMiddleware(typeof(AnalyzerExample.Products.Middleware.ProductQueryCacheMiddleware<,>));
    
    // Register notification middleware
    config.AddNotificationMiddleware<AnalyzerExample.Common.Middleware.GlobalNotificationMiddleware>();
    config.AddNotificationMiddleware(typeof(AnalyzerExample.Common.Middleware.DomainEventMiddleware<>));
    
},
// Discover from all related assemblies
typeof(AnalyzerExample.Common.Domain.BaseEntity).Assembly,      // Common
typeof(AnalyzerExample.Products.Domain.Product).Assembly,       // Products  
typeof(AnalyzerExample.Users.Domain.User).Assembly,             // Users
typeof(AnalyzerExample.Orders.Domain.Order).Assembly,           // Orders
typeof(Program).Assembly                                         // Main
);
```

### Fully Automated Configuration
```csharp
// Register Blazing.Mediator with comprehensive discovery across all assemblies
services.AddMediator(config =>
{
    // Enable detailed statistics tracking for comprehensive analysis
    config.WithStatisticsTracking();

    // Enable middleware discovery
    config.WithMiddlewareDiscovery();

    // Enable notification middleware discovery
    config.WithNotificationMiddlewareDiscovery();

    // Register assemblies for handler and middleware discovery
    config.AddAssemblies(
            typeof(Common.Domain.BaseEntity).Assembly, // Common
            typeof(Products.Domain.Product).Assembly, // Products  
            typeof(Users.Domain.User).Assembly, // Users
            typeof(Orders.Domain.Order).Assembly, // Orders
            typeof(Program).Assembly // Main
        );
});
```

## Multi-Assembly Learning Points

1. **Cross-Assembly Analysis**: How to analyze mediator usage across multiple projects and assemblies
2. **Domain Separation**: Clean separation of concerns across business domains  
3. **Shared Infrastructure**: Common interfaces and middleware across domain boundaries
4. **Assembly Discovery**: Automatic handler and middleware discovery across multiple assemblies
5. **Namespace Organization**: Clear namespace organization for better code organization
6. **Type Normalization Across Assemblies**: Consistent type formatting regardless of source assembly
7. **Distributed Architecture**: Building scalable, modular applications with clean dependencies
8. **Missing Handler Detection**: Using analyzers to identify missing components across complex solutions

## Architecture Benefits Demonstrated

### **Modular Design**
- Clear domain boundaries with separate assemblies
- Shared common infrastructure for cross-cutting concerns
- Independent deployment and versioning capabilities
- Testable, maintainable code structure

### **Analysis Capabilities**
- Cross-assembly type normalization without backticks
- Assembly and namespace identification across projects
- Middleware pipeline analysis spanning multiple assemblies
- Handler discovery and validation across domain boundaries
- Generic type parameter analysis in complex inheritance hierarchies
- **Missing handler detection across all projects**

### **Type Normalization Excellence**
- Consistent formatting across all assemblies and namespaces
- Clean display of complex domain-specific generic types
- Assembly distribution statistics and insights
- Before/after comparisons showing normalization improvements
- Summary formats optimized for multi-project scenarios

## Extension Methods Reference

### QueryCommandAnalysisExtensions

**Type Normalization Methods:**
- `NormalizeResponseTypeName()` - Clean response type names without backticks
- `NormalizePrimaryInterfaceName()` - Clean interface names with proper generic syntax
- `NormalizeHandlerNames()` - Clean handler type names across assemblies
- `NormalizeHandlerDetails()` - Normalized handler information strings

**Fully Qualified Methods:**
- `GetFullyQualifiedResponseTypeName()` - Full namespace and assembly info for response types
- `GetFullyQualifiedPrimaryInterfaceName()` - Full namespace info for interfaces
- `GetFullyQualifiedHandlerNames()` - Full namespace info for handlers

### MiddlewareAnalysisExtensions

**Type Normalization Methods:**
- `NormalizeTypeName()` - Clean middleware type names without backticks
- `NormalizeClassName()` - Clean class names without generic suffixes
- `NormalizeTypeParameters()` - Clean generic parameter formatting
- `NormalizeGenericConstraints()` - Clean generic constraint formatting
- `NormalizeOrderDisplay()` - Special order value formatting (int.MinValue, etc.)
- `NormalizeSummary(includeNamespace)` - Complete normalized summaries

**Analysis Methods:**
- `GetAssemblyName()` - Assembly name identification
- `GetNamespace()` - Namespace identification
- `IsGeneric()` - Generic type detection
- `GetGenericParameterCount()` - Generic parameter counting
- `HasConfiguration()` - Configuration detection
- `GetConfigurationTypeName()` - Configuration type names

### NotificationAnalysisExtensions

**Type Normalization Methods:**
- `NormalizeHandlerNames()` - Clean notification handler names
- `NormalizeHandlerDetails()` - Normalized handler information
- `NormalizePrimaryInterfaceName()` - Clean notification interface names
- `NormalizeSubscriberNames()` - Clean subscriber type names

**Fully Qualified Methods:**
- `GetFullyQualifiedHandlerNames()` - Full namespace info for notification handlers
- `GetFullyQualifiedPrimaryInterfaceName()` - Full namespace info for notification interfaces

This example serves as both a comprehensive demonstration of Blazing.Mediator's cross-assembly analysis capabilities and a practical reference for building scalable, modular applications with clean type normalization across multiple projects and domains. **The intentionally missing handlers showcase how these analyzer tools are essential for debugging and maintaining complex multi-project solutions.**
