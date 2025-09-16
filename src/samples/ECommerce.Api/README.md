# ECommerce.Api - Blazing.Mediator E-Commerce Demo with Real-Time Statistics

A comprehensive demonstration of the Blazing.Mediator library showcasing CQRS patterns, middleware pipelines, notifications, and **real-time mediator statistics tracking** through a realistic e-commerce scenario.

## ?? Table of Contents

- [??? Architecture](#?-architecture)
- [?? Design Principles](#-design-principles)
- [?? E-Commerce Features Demonstrated](#-e-commerce-features-demonstrated)
  - [?? CQRS Implementation](#-cqrs-implementation)
  - [?? Middleware Pipeline](#-middleware-pipeline)
  - [?? Notification System](#-notification-system)
  - [? Validation](#-validation)
  - [??? Error Handling](#?-error-handling)
  - [?? **Real-Time Statistics Tracking**](#-real-time-statistics-tracking)
- [?? E-Commerce Components Overview](#-e-commerce-components-overview)
  - [Products](#products)
  - [Orders](#orders)
  - [Notifications](#notifications)
  - [Statistics & Analysis](#statistics--analysis)
  - [Background Services](#background-services)
- [????? Running the Example](#?-running-the-example)
- [?? API Endpoints](#-api-endpoints)
  - [Product Endpoints](#product-endpoints)
  - [Order Endpoints](#order-endpoints)
  - [**?? Special Mediator Statistics Endpoints**](#-special-mediator-statistics-endpoints)
  - [**?? Mediator Analysis Endpoints**](#-mediator-analysis-endpoints)
- [?? **Real-Time Statistics Features**](#-real-time-statistics-features)
  - [Session-Based Tracking](#session-based-tracking)
  - [Global Statistics](#global-statistics)
  - [Statistics Middleware](#statistics-middleware)
  - [Automatic Cleanup](#automatic-cleanup)
- [?? Notification System](#-notification-system-1)
  - [Product Notifications](#product-notifications)
  - [Order Notifications](#order-notifications)
  - [Email Integration](#email-integration)
  - [Inventory Management](#inventory-management)
- [?? **Unique Features**](#-unique-features)
- [?? Key Learnings](#-key-learnings)
- [?? Technologies Used](#-technologies-used)
- [?? Further Reading](#-further-reading)

## ??? Architecture

This project follows **CQRS** (Command Query Responsibility Segregation) principles and **Clean Architecture** patterns with advanced features:

```
ECommerce.Api/
??? Application/
?   ??? Commands/          # Write operations (CreateProduct, CreateOrder, etc.)
?   ??? Queries/           # Read operations (GetProducts, GetOrders, etc.)
?   ??? Handlers/          # Business logic processors
?   ??? Notifications/     # Domain events and notifications
?   ??? Middleware/        # Cross-cutting concerns (Logging, etc.)
?   ??? Validators/        # FluentValidation rules
?   ??? Services/          # Background services (Email, Inventory)
?   ??? DTOs/             # Data Transfer Objects
?   ??? Mappings/         # Entity to DTO mappings
??? Controllers/          # MVC Controllers
?   ??? ProductsController.cs     # Product management
?   ??? OrdersController.cs       # Order management
?   ??? MediatorController.cs     # **?? Statistics & Analysis**
??? Domain/              # Domain entities
??? Infrastructure/      # Data layer (EF Core DbContext)
??? Services/            # **Statistics tracking services**
??? Middleware/          # **Statistics tracking middleware**
??? Program.cs          # Application entry point
```

## ?? Design Principles

This example adheres to advanced software engineering principles:

- **CQRS**: Complete separation of read and write operations
- **Domain-Driven Design**: Rich domain models with business logic
- **Event-Driven Architecture**: Notifications for decoupled communication
- **Clean Architecture**: Organized layers with proper dependencies
- **SOLID Principles**: All five principles demonstrated
- **Real-Time Monitoring**: Live statistics and performance tracking

## ?? E-Commerce Features Demonstrated

### ? Powerful Auto-Registration

- **Simple Setup**: Single-line mediator registration with automatic discovery
- **Handler Auto-Discovery**: Automatically finds and registers all implementations
- **Middleware Auto-Registration**: Seamlessly discovers and registers middleware
- **Statistics Tracking**: Built-in real-time statistics collection
- **Zero Configuration**: Works out-of-the-box with conventional patterns

### ?? CQRS Implementation

**Commands**: Operations that modify state
- `CreateProductCommand` - Product creation with validation
- `UpdateProductCommand` - Product information updates
- `UpdateProductStockCommand` - Inventory management
- `DeactivateProductCommand` - Product lifecycle management
- `CreateOrderCommand` - Order creation with complex validation
- `ProcessOrderCommand` - Complete order processing workflow
- `UpdateOrderStatusCommand` - Order status management
- `CancelOrderCommand` - Order cancellation

**Queries**: Operations that retrieve data
- `GetProductByIdQuery` - Product details retrieval
- `GetProductsQuery` - Paginated product listing with search
- `GetLowStockProductsQuery` - Inventory monitoring
- `GetOrderByIdQuery` - Order details with items
- `GetOrdersQuery` - Paginated order listing with filtering
- `GetCustomerOrdersQuery` - Customer-specific order history
- `GetOrderStatisticsQuery` - Business intelligence and metrics

### ?? Middleware Pipeline

- **ProductLoggingMiddleware**: Product operation logging
- **OrderLoggingMiddleware**: Order operation tracking
- **NotificationLoggingMiddleware**: Notification processing logging
- **StatisticsTrackingMiddleware**: **Real-time statistics collection**
- **SessionTrackingMiddleware**: **Session-based statistics tracking**

### ?? Notification System

Rich domain event system with multiple subscribers:

**Product Events:**
- `ProductCreatedNotification` - New product announcements
- `ProductUpdatedNotification` - Product changes
- `ProductStockLowNotification` - Inventory alerts
- `ProductOutOfStockNotification` - Stock depletion alerts

**Order Events:**
- `OrderCreatedNotification` - New order processing
- `OrderStatusChangedNotification` - Order lifecycle tracking

### ? Validation

- **FluentValidation**: Comprehensive validation rules
- **Business Rule Validation**: Complex e-commerce business logic
- **Error Aggregation**: Detailed validation error reporting

### ??? Error Handling

- **Custom Exception Types**: Domain-specific exceptions
- **Global Exception Handling**: Structured error responses
- **Validation Error Details**: Comprehensive error information

### ?? **Real-Time Statistics Tracking**

**?? UNIQUE FEATURE**: This project includes a complete real-time statistics tracking system:

- **Session-Based Tracking**: Statistics per user session
- **Global Statistics**: Application-wide usage metrics
- **Live Updates**: Real-time statistics that update as requests are processed
- **Type Analysis**: Comprehensive analysis of queries, commands, and handlers
- **Performance Monitoring**: Execution tracking and metrics
- **Automatic Cleanup**: Background cleanup of inactive sessions

## ?? E-Commerce Components Overview

### Products

**Commands & Handlers:**
- Product CRUD operations with validation
- Stock management and inventory tracking
- Product lifecycle management (activation/deactivation)
- Bulk operations for testing scenarios

**Queries & Handlers:**
- Product catalog browsing with pagination
- Search and filtering capabilities
- Low stock monitoring and alerts
- Product performance metrics

### Orders

**Commands & Handlers:**
- Complete order processing workflow
- Order status management with notifications
- Order cancellation with business rules
- Customer order history management

**Queries & Handlers:**
- Order retrieval with item details
- Customer order history
- Order statistics and business intelligence
- Advanced filtering and search

### Notifications

**Notification Handlers:**
- **EmailNotificationService**: Background email processing
- **InventoryManagementService**: Stock monitoring and alerts
- **Order Processing**: Automated order workflow notifications
- **Business Events**: Domain event processing

### Statistics & Analysis

**?? Special Controllers:**
- **MediatorController**: **Real-time statistics and analysis endpoints**
- **Statistics Tracking**: Live usage monitoring
- **Session Management**: Per-user statistics tracking
- **Type Analysis**: Comprehensive mediator introspection

### Background Services

- **EmailNotificationService**: Handles email notifications
- **InventoryManagementService**: Monitors stock levels
- **StatisticsCleanupService**: Cleans up inactive sessions

## ????? Running the Example

### Prerequisites

- .NET 9.0 or later
- Terminal/Command Prompt or Visual Studio

### Steps

1. Navigate to the example directory:
   ```bash
   cd src/samples/ECommerce.Api
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. Open your browser and navigate to:
   - **Swagger UI**: `https://localhost:5001/swagger` (HTTPS)
   - **API Base**: `https://localhost:5001/api` (HTTPS)
   - **HTTP Alternative**: `http://localhost:5000` (HTTP)

## ?? API Endpoints

### Product Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/products/{id}` | Get product by ID |
| `GET` | `/api/products` | Get paginated products list |
| `GET` | `/api/products/low-stock` | Get low stock products |
| `POST` | `/api/products` | Create new product |
| `PUT` | `/api/products/{id}` | Update product |
| `PUT` | `/api/products/{id}/stock` | Update product stock |
| `POST` | `/api/products/{id}/deactivate` | Deactivate product |
| `POST` | `/api/products/{id}/reduce-stock` | Reduce stock (demo) |
| `POST` | `/api/products/{id}/simulate-bulk-order` | Simulate bulk order (demo) |

### Order Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/orders/{id}` | Get order by ID |
| `GET` | `/api/orders` | Get paginated orders list |
| `GET` | `/api/orders/customer/{customerId}` | Get customer orders |
| `GET` | `/api/orders/statistics` | Get order statistics |
| `POST` | `/api/orders` | Create new order |
| `POST` | `/api/orders/process` | Process complete order |
| `PUT` | `/api/orders/{id}/status` | Update order status |
| `POST` | `/api/orders/{id}/cancel` | Cancel order |
| `POST` | `/api/orders/{id}/complete` | Complete order workflow |
| `POST` | `/api/orders/{id}/process-workflow` | Process full order workflow |

### **?? Special Mediator Statistics Endpoints**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/mediator/session` | **Get current session ID** |
| `GET` | `/api/mediator/statistics` | **Real-time global statistics** |
| `GET` | `/api/mediator/statistics/session/{id}` | **Session-specific statistics** |
| `GET` | `/api/mediator/statistics/sessions` | **All active sessions** |

### **?? Mediator Analysis Endpoints**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/mediator/analyze/queries` | **Analyze all queries** |
| `GET` | `/api/mediator/analyze/commands` | **Analyze all commands** |
| `GET` | `/api/mediator/analyze` | **Complete mediator analysis** |

## ?? **Real-Time Statistics Features**

### Session-Based Tracking

**Get Your Session ID:**
```bash
GET /api/mediator/session
```

**Response:**
```json
{
  "message": "Current Session ID",
  "sessionId": "stats_1701234567_abc12345",
  "note": "This session ID is used for tracking your mediator statistics across requests",
  "usage": {
    "viewSessionStats": "GET /api/mediator/statistics/session/stats_1701234567_abc12345",
    "viewGlobalStats": "GET /api/mediator/statistics",
    "viewAllSessions": "GET /api/mediator/statistics/sessions"
  },
  "sessionInfo": {
    "sessionAvailable": true,
    "aspNetCoreSessionId": "...",
    "statisticsSessionId": "stats_1701234567_abc12345",
    "sessionKeys": ["MediatorStatisticsSessionId"]
  }
}
```

### Global Statistics

**Get Real-Time Global Statistics:**
```bash
GET /api/mediator/statistics
```

**Response:**
```json
{
  "message": "Real-Time Mediator Statistics",
  "note": "These statistics update dynamically as requests are processed",
  "globalStatistics": {
    "summary": {
      "uniqueQueryTypes": 7,
      "uniqueCommandTypes": 8,
      "uniqueNotificationTypes": 6,
      "totalQueryExecutions": 45,
      "totalCommandExecutions": 23,
      "totalNotificationExecutions": 31,
      "activeSessions": 3
    },
    "details": {
      "queryTypes": {
        "GetProductByIdQuery": 15,
        "GetProductsQuery": 8,
        "GetOrderByIdQuery": 12,
        "GetOrdersQuery": 5,
        "GetLowStockProductsQuery": 3,
        "GetCustomerOrdersQuery": 1,
        "GetOrderStatisticsQuery": 1
      },
      "commandTypes": {
        "CreateProductCommand": 5,
        "UpdateProductCommand": 3,
        "CreateOrderCommand": 8,
        "UpdateOrderStatusCommand": 4,
        "UpdateProductStockCommand": 2,
        "ProcessOrderCommand": 1
      },
      "notificationTypes": {
        "ProductCreatedNotification": 5,
        "OrderCreatedNotification": 8,
        "OrderStatusChangedNotification": 12,
        "ProductStockLowNotification": 3,
        "ProductOutOfStockNotification": 2,
        "ProductUpdatedNotification": 1
      }
    }
  },
  "trackingInfo": {
    "method": "Real-time tracking via StatisticsTrackingMiddleware with session persistence",
    "scope": "Global statistics track all application usage, session statistics track per-user activity",
    "sessionTracking": "Enabled - tracks per session/user statistics using ASP.NET Core session state",
    "autoCleanup": "Inactive sessions are automatically cleaned up after 2 hours"
  }
}
```

### Statistics Middleware

The `StatisticsTrackingMiddleware` automatically tracks:

- **Query Executions**: Every query sent through the mediator
- **Command Executions**: Every command sent through the mediator  
- **Notification Publications**: Every notification published
- **Session Association**: Links activity to user sessions
- **Type Classification**: Distinguishes between queries, commands, and notifications

### Automatic Cleanup

The `StatisticsCleanupService` runs in the background:

- **Scheduled Cleanup**: Every 30 minutes
- **Session Timeout**: 2 hours of inactivity
- **Memory Management**: Prevents memory leaks from inactive sessions
- **Configurable**: Cleanup interval and timeout are configurable

## ?? Notification System

### Product Notifications

**Product Created:**
```csharp
// Automatically published when products are created
public record ProductCreatedNotification(int ProductId, string ProductName, decimal Price) : INotification;
```

**Stock Alerts:**
```csharp
// Published when stock levels are low
public record ProductStockLowNotification(int ProductId, string ProductName, int CurrentStock, int Threshold) : INotification;

// Published when products are out of stock
public record ProductOutOfStockNotification(int ProductId, string ProductName) : INotification;
```

### Order Notifications

**Order Processing:**
```csharp
// Published when orders are created
public record OrderCreatedNotification(int OrderId, int CustomerId, string CustomerEmail, decimal TotalAmount, List<OrderItemDto> Items) : INotification;

// Published when order status changes
public record OrderStatusChangedNotification(int OrderId, OrderStatus OldStatus, OrderStatus NewStatus, DateTime ChangedAt) : INotification;
```

### Email Integration

The `EmailNotificationService` handles:
- Order confirmation emails
- Stock alert notifications
- Customer communication
- Background email processing

### Inventory Management

The `InventoryManagementService` provides:
- Automatic stock monitoring
- Low stock alerts
- Out of stock notifications
- Inventory level tracking

## ?? **Unique Features**

This ECommerce.Api project includes several unique features not found in other sample projects:

### ?? **Real-Time Statistics System**
- Live tracking of mediator usage
- Session-based statistics
- Global application metrics
- Automatic cleanup and memory management

### ?? **Comprehensive Analysis APIs**
- Deep introspection of mediator setup
- Handler status verification
- Type analysis and discovery
- Development and debugging tools

### ?? **Session Management**
- Per-user session tracking
- Session persistence across requests
- Session lifecycle management
- Multi-user statistics isolation

### ?? **Rich Notification System**
- Multiple notification types
- Background service processing
- Email integration
- Event-driven architecture

### ??? **Development Tools**
- Middleware pipeline inspection
- Handler registration verification
- Statistics visualization
- Debugging endpoints

## ?? Key Learnings

This example teaches:

- **CQRS Pattern Implementation**: Complete separation of read/write operations
- **Real-Time Statistics**: Live monitoring and tracking systems
- **Session Management**: Per-user session tracking and persistence
- **Notification Systems**: Event-driven architecture with multiple subscribers
- **Background Services**: Long-running background processing
- **Complex Domain Logic**: E-commerce business rules and workflows
- **API Design**: RESTful endpoints with proper HTTP semantics
- **Statistics Middleware**: Cross-cutting concern implementation
- **Memory Management**: Automatic cleanup and resource management
- **Performance Monitoring**: Real-time usage tracking and metrics
- **Type Analysis**: Reflection-based introspection and analysis
- **Development Tools**: Building debugging and analysis endpoints
- **Clean Architecture**: Organized, maintainable code structure
- **Domain Events**: Rich domain event modeling
- **Error Handling**: Comprehensive exception handling strategies
- **Validation**: Complex business rule validation
- **Entity Relationships**: Complex domain entity modeling
- **Data Access**: Advanced Entity Framework patterns
- **Dependency Injection**: Advanced DI container usage
- **Configuration**: Flexible application configuration
- **Testing**: Integration testing with statistics verification

## ?? Technologies Used

- **.NET 9.0**: Latest .NET framework
- **Blazing.Mediator**: CQRS and mediator pattern implementation with statistics
- **ASP.NET Core**: Web API framework
- **Entity Framework Core**: Object-relational mapping
- **FluentValidation**: Declarative validation library
- **Swagger/OpenAPI**: API documentation
- **Background Services**: IHostedService for background processing
- **Session State**: ASP.NET Core session management
- **Microsoft.Extensions.Hosting**: Application hosting and dependency injection
- **Microsoft.Extensions.Logging**: Structured logging

## ?? Further Reading

- [Blazing.Mediator Documentation](../../docs/)
- [CQRS Pattern Guide](../../docs/MEDIATOR_PATTERN_GUIDE.md)
- [Notification Guide](../../docs/NOTIFICATION_GUIDE.md)
- [ASP.NET Core Web APIs](https://docs.microsoft.com/en-us/aspnet/core/web-api/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Background Services in .NET](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)