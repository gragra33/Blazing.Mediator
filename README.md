# Blazing.Mediator with Sample Projects

This repository demonstrates the implementation of the Mediator pattern with CQRS using the `Blazing.Mediator` library. It includes a lightweight library implementation and two comprehensive sample projects.

## 📚 Documentation

For complete implementation instructions, CQRS concepts, and best practices, see the [**📖 Mediator Pattern Implementation Guide**](docs/MEDIATOR_PATTERN_GUIDE.md).

## Projects Structure

```
├── src/
│   └── Blazing.Mediator/          # Core mediator library
├── samples/
│   ├── UserManagement.Api/        # User management CRUD operations
│   │   └── UserManagement.http    # Ready-to-use HTTP requests
│   └── ECommerce.Api/             # E-commerce order management system
│       └── ECommerce.http         # Ready-to-use HTTP requests
├── docs/
│   ├── README.md                  # Documentation index
│   └── MEDIATOR_PATTERN_GUIDE.md  # Comprehensive implementation guide
└── Blazing.Mediator.sln           # Solution file
```

## Quick Start

### Prerequisites

-   .NET 9.0 SDK
-   Visual Studio 2022 or VS Code
-   SQL Server LocalDB (for production mode) or In-Memory database (development mode)

### Installation

Add the Blazing.Mediator NuGet package to your project.

#### Install the package via .NET CLI or the NuGet Package Manager.

##### .NET CLI

```bash
dotnet add package Blazing.Mediator
```

##### NuGet Package Manager

```bash
Install-Package Blazing.Mediator
``` 

#### Manually adding to your project

```xml
<PackageReference Include="Blazing.Mediator" Version="1.0.0" />
```

### Running the Sample Projects

#### Option 1: Visual Studio

1. Open `Blazing.Mediator.sln` in Visual Studio
2. Set either `UserManagement.Api` or `ECommerce.Api` as the startup project
3. Press F5 to run

#### Option 2: Command Line

```powershell
# Clone or navigate to the project directory
cd "c:\dev\Blazing.Mediator"

# Build the solution
dotnet build

# Run User Management API
cd samples\UserManagement.Api
dotnet run

# Or run E-commerce API
cd samples\ECommerce.Api
dotnet run
```

Both projects will start with Swagger UI available at `https://localhost:7xxx/swagger`

## Testing the APIs

Each sample project includes comprehensive `.http` files with ready-to-use HTTP requests:

-   **UserManagement.Api**: `UserManagement.http` - Contains all user management endpoints
-   **ECommerce.Api**: `ECommerce.http` - Contains all product and order management endpoints

These files can be used with:

-   **VS Code REST Client extension**
-   **JetBrains HTTP Client** (IntelliJ IDEA, WebStorm, etc.)
-   **Postman** (import the requests)
-   Any HTTP client that supports `.http` format

The `.http` files include:

-   ✅ All available endpoints with sample data
-   ✅ Query parameters and filtering examples
-   ✅ Error scenarios and validation testing
-   ✅ Batch operations for creating test data
-   ✅ Comprehensive documentation and comments

## Sample Projects Overview

### 1. UserManagement.Api

A comprehensive user management system demonstrating CQRS patterns:

**Features:**

-   ✅ User CRUD operations
-   ✅ Pagination and filtering
-   ✅ User activation/deactivation
-   ✅ User statistics queries
-   ✅ FluentValidation integration
-   ✅ Manual mapping with extension methods (no AutoMapper dependency)
-   ✅ Comprehensive error handling
-   ✅ Minimal API implementation for clean, functional endpoints

**Key Endpoints:**

-   `GET /api/users` - Get paginated users with filtering
-   `GET /api/users/{id}` - Get user by ID
-   `GET /api/users/active` - Get all active users
-   `GET /api/users/{id}/statistics` - Get user statistics
-   `POST /api/users` - Create new user
-   `PUT /api/users/{id}` - Update user
-   `DELETE /api/users/{id}` - Delete user
-   `POST /api/users/{id}/activate` - Activate user account
-   `POST /api/users/{id}/deactivate` - Deactivate user account

### 2. ECommerce.Api

An e-commerce order management system showcasing advanced CQRS scenarios:

**Features:**

-   ✅ Product catalog management
-   ✅ Order processing with inventory management
-   ✅ Stock validation and reservation
-   ✅ Order status tracking
-   ✅ Sales analytics and reporting
-   ✅ Composite command handlers
-   ✅ Advanced error handling with operation results
-   ✅ Traditional MVC Controller implementation

**Key Endpoints:**

**Products:**

-   `GET /api/products` - Get products with filtering
-   `GET /api/products/{id}` - Get product by ID
-   `GET /api/products/low-stock` - Get low stock products
-   `POST /api/products` - Create product
-   `PUT /api/products/{id}` - Update product
-   `PUT /api/products/{id}/stock` - Update product stock

**Orders:**

-   `GET /api/orders` - Get orders with filtering
-   `GET /api/orders/{id}` - Get order by ID
-   `GET /api/orders/customer/{customerId}` - Get customer orders
-   `GET /api/orders/statistics` - Get order statistics
-   `POST /api/orders` - Create order
-   `POST /api/orders/process` - Process complete order (composite operation)
-   `PUT /api/orders/{id}/status` - Update order status
-   `POST /api/orders/{id}/cancel` - Cancel order

## Database Configuration

### Development Mode (Default)

Both projects use **In-Memory Database** by default for easy testing:

-   No setup required
-   Data is seeded automatically
-   Data is lost when application stops

### Production Mode

To use SQL Server LocalDB:

1. Update `appsettings.json`:

```json
{
    "ConnectionStrings": {
        "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=YourDbName;Trusted_Connection=true;MultipleActiveResultSets=true"
    }
}
```

2. Set environment:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
```

3. Run migrations:

```powershell
dotnet ef database update
```

## Key CQRS Implementations Demonstrated

### 1. Clear Command/Query Separation

-   **Commands**: `CreateUserCommand`, `UpdateOrderStatusCommand`, `ProcessOrderCommand`
-   **Queries**: `GetUserByIdQuery`, `GetOrderStatisticsQuery`, `GetProductsQuery`

### 2. Optimized Read Operations

-   Read-only queries use `AsNoTracking()`
-   Pagination and filtering
-   Projection to DTOs
-   Caching strategies (in UserManagement example)

### 3. Business Logic in Command Handlers

-   Domain entity methods
-   Validation with FluentValidation
-   Transaction management
-   Error handling with custom exceptions

### 4. Composite Operations

-   `ProcessOrderCommand` orchestrates multiple operations
-   Calls other commands through mediator
-   Demonstrates command composition patterns

### 5. Advanced Scenarios

-   Operation results with success/failure status
-   Background processing patterns
-   Domain events (structure prepared)
-   Repository pattern separation

## Testing the APIs

### Using .http Files (Recommended)

Both sample projects include comprehensive `.http` files for easy API testing:

**UserManagement.Api** - `samples/UserManagement.Api/UserManagement.http`:

```http
# Get all users with pagination
GET {{baseUrl}}/api/users?page=1&pageSize=10&includeInactive=false

# Create new user
POST {{baseUrl}}/api/users
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "dateOfBirth": "1990-05-15T00:00:00"
}
```

**ECommerce.Api** - `samples/ECommerce.Api/ECommerce.http`:

```http
# Get all products
GET {{baseUrl}}/api/products?page=1&pageSize=10

# Create new product
POST {{baseUrl}}/api/products
Content-Type: application/json

{
  "name": "Gaming Laptop",
  "description": "High-performance gaming laptop",
  "price": 1299.99,
  "stockQuantity": 15
}
```

**To use the .http files:**

1. Open the `.http` file in VS Code with REST Client extension
2. Update the `@baseUrl` variable if needed
3. Click "Send Request" above any HTTP request
4. View the response in the adjacent panel

### Sample cURL Commands

#### User Management API

```bash
# Create a user
curl -X POST "https://localhost:7001/api/users" \
-H "Content-Type: application/json" \
-d '{
  "firstName": "Alice",
  "lastName": "Johnson",
  "email": "alice.johnson@example.com",
  "dateOfBirth": "1990-05-15T00:00:00"
}'

# Get users with filtering
curl "https://localhost:7001/api/users?page=1&pageSize=5&searchTerm=alice&includeInactive=false"

# Get user statistics
curl "https://localhost:7001/api/users/1/statistics"
```

#### E-commerce API

```bash
# Create a product
curl -X POST "https://localhost:7002/api/products" \
-H "Content-Type: application/json" \
-d '{
  "name": "Gaming Keyboard",
  "description": "Mechanical RGB gaming keyboard",
  "price": 89.99,
  "stockQuantity": 50
}'

# Create an order
curl -X POST "https://localhost:7002/api/orders" \
-H "Content-Type: application/json" \
-d '{
  "customerId": 1,
  "customerEmail": "customer@example.com",
  "shippingAddress": "123 Main St, City, State 12345",
  "items": [
    {"productId": 1, "quantity": 1},
    {"productId": 2, "quantity": 2}
  ]
}'

# Get order statistics
curl "https://localhost:7002/api/orders/statistics"
```

## Learning Resources

### 1. Implementation Guide

Read the comprehensive [MEDIATOR_PATTERN_GUIDE.md](docs/MEDIATOR_PATTERN_GUIDE.md) for:

-   CQRS concepts and benefits
-   Step-by-step implementation instructions
-   Best practices and patterns
-   Testing strategies
-   Real-world examples

### 2. Code Examples

Explore the sample projects to see:

-   Handler organization and structure
-   Validation patterns
-   Error handling strategies
-   Repository patterns
-   Minimal API implementations (UserManagement) and Controller implementations (ECommerce)
-   **Ready-to-use .http files** for comprehensive API testing

### 3. Architecture Patterns

The samples demonstrate:

-   Clean Architecture principles
-   Domain-Driven Design (DDD) concepts
-   SOLID principles
-   Dependency Injection patterns
-   Separation of Concerns

## Extending the Samples

### Adding Cross-Cutting Concerns

-   **Logging**: Already integrated via ILogger
-   **Caching**: Implement caching decorators for query handlers
-   **Authorization**: Add authorization policies to minimal APIs or attributes to controllers
-   **Audit Logging**: Implement audit trail in command handlers
-   **Performance Monitoring**: Add performance counters to handlers

## Additional Notes

### Error Handling

-   Custom exceptions for domain errors
-   Validation exceptions with detailed error messages
-   Global exception handling in minimal APIs and controllers
-   Operation result patterns for complex scenarios

### Testing Strategy

-   Unit test handlers in isolation
-   Integration tests for complete flows
-   Mock repositories for testing
-   Validation rule testing

This implementation provides a solid foundation for building scalable applications using the Mediator pattern with CQRS principles.
