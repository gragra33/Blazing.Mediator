# UserManagement.Api - Blazing.Mediator User Management Demo with Real-Time Statistics

A comprehensive demonstration of the Blazing.Mediator library showcasing CQRS patterns, validation, error handling, **real-time mediator statistics tracking**, and **pure minimal API implementation** through a realistic user management scenario.

## ?? Table of Contents

- [??? Architecture](#?-architecture)
- [?? Design Principles](#-design-principles)
- [?? Features Demonstrated](#-features-demonstrated)
  - [?? CQRS Implementation](#-cqrs-implementation)
  - [?? Middleware Pipeline](#-middleware-pipeline)
  - [? Validation](#-validation)
  - [??? Error Handling](#?-error-handling)
  - [?? Pure Minimal API Implementation](#-pure-minimal-api-implementation)
  - [?? **Real-Time Statistics Tracking**](#-real-time-statistics-tracking)
- [?? Components Overview](#-components-overview)
- [????? Running the Example](#?-running-the-example)
- [?? API Endpoints](#-api-endpoints)
  - [User Management Endpoints](#user-management-endpoints)
  - [**?? Real-Time Statistics Endpoints**](#-real-time-statistics-endpoints)
  - [**?? Analysis & Health Endpoints**](#-analysis--health-endpoints)
- [?? **Real-Time Statistics Features**](#-real-time-statistics-features)
- [?? Session Tracking](#-session-tracking)
- [?? **Unique Features**](#-unique-features)
- [?? Key Learnings](#-key-learnings)
- [? **Test Coverage**](#-test-coverage)
- [?? Technologies Used](#-technologies-used)
- [?? Further Reading](#-further-reading)

## ??? Architecture

This project follows **CQRS** (Command Query Responsibility Segregation) principles and **Clean Architecture** patterns with a **pure minimal API approach**:

```
UserManagement.Api/
??? Application/
?   ??? Commands/          # Write operations (CreateUser, UpdateUser, etc.)
?   ??? Queries/           # Read operations (GetUserById, GetUsers, etc.)
?   ??? Handlers/          # Business logic processors
?   ??? Validators/        # FluentValidation rules
?   ??? Middleware/        # Mediator pipeline middleware
?   ??? DTOs/             # Data Transfer Objects
?   ??? Exceptions/       # Custom exception types
??? Endpoints/            # ?? Minimal API endpoint definitions (NO Controllers)
??? Services/            # ?? Statistics tracking services
??? Middleware/          # ?? ASP.NET Core middleware (session tracking)
??? Extensions/          # Service registration and configuration
??? Infrastructure/      # Data layer (EF Core)
??? Domain/              # Domain entities
??? Program.cs          # Application entry point
```

## ?? Design Principles

This example demonstrates modern .NET development practices:

- **Pure Minimal APIs** - No controllers, all endpoints use minimal API patterns
- **CQRS** - Clean separation of commands and queries
- **SOLID Principles** - Single responsibility, dependency inversion, etc.
- **Real-Time Tracking** - Live statistics collection and monitoring
- **Session Management** - Per-user session-based tracking
- **Auto-Registration** - Automatic handler and middleware discovery
- **Clean Architecture** - Well-organized, maintainable code structure

## ?? Features Demonstrated

### ? Powerful Auto-Registration

- **Single-Line Setup**: Complete mediator registration with automatic discovery
- **Handler Auto-Discovery**: Automatically finds and registers all implementations
- **Middleware Auto-Registration**: Seamlessly discovers and registers middleware
- **Statistics Integration**: Built-in real-time statistics collection
- **Zero Configuration**: Works out-of-the-box

### ?? CQRS Implementation

**Commands** (Write Operations):
- `CreateUserCommand` - User creation with validation
- `CreateUserWithIdCommand` - User creation with explicit ID
- `UpdateUserCommand` - User information updates
- `UpdateUserWithResultCommand` - Updates with operation result
- `ActivateUserAccountCommand` - Account activation
- `DeactivateUserAccountCommand` - Account deactivation
- `DeleteUserCommand` - User deletion

**Queries** (Read Operations):
- `GetUserByIdQuery` - Retrieve specific user
- `GetUsersQuery` - Paginated user listing with search/filtering
- `GetActiveUsersQuery` - Active users only
- `GetUserStatisticsQuery` - User metrics and statistics

### ?? Middleware Pipeline

**Mediator Middleware** (registered with mediator):
- `GeneralLoggingMiddleware<,>` - Request/response logging
- `GeneralCommandLoggingMiddleware<>` - Command-specific logging
- `StatisticsTrackingMiddleware<,>` - **Real-time statistics for typed requests**
- `StatisticsTrackingVoidMiddleware<>` - **Statistics for void commands**

**ASP.NET Core Middleware** (HTTP pipeline):
- `SessionTrackingMiddleware` - **Session-based statistics tracking**

### ? Validation

- **FluentValidation Integration** - Declarative validation rules
- **Multi-Validator Support** - Multiple validators per command
- **Error Aggregation** - Comprehensive validation error reporting
- **Real-world Rules** - Email validation, required fields, length constraints

### ??? Error Handling

- **Custom Exceptions** - Typed exceptions for different scenarios
- **Global Exception Handling** - Structured error responses
- **Validation Error Details** - Detailed validation feedback
- **HTTP Status Codes** - Proper RESTful error responses

### ?? Pure Minimal API Implementation

**?? Key Difference from ECommerce.Api**: This project uses **only minimal APIs** - no controllers!

- **UserQueryEndpoints** - All query operations using minimal APIs
- **UserCommandEndpoints** - All command operations using minimal APIs
- **MediatorAnalysisEndpoints** - Statistics and analysis using minimal APIs
- **Modern Pattern** - Leverages latest ASP.NET Core minimal API features
- **Consistent Design** - Single endpoint pattern throughout

### ?? **Real-Time Statistics Tracking**

**?? FULLY WORKING**: Complete real-time statistics system with session tracking:

- **Live Session Tracking** - Per-user session statistics
- **Global Metrics** - Application-wide usage statistics
- **Real-Time Updates** - Statistics update as requests are processed
- **Type Analysis** - Comprehensive query/command analysis
- **Automatic Cleanup** - Background cleanup of inactive sessions

## ?? Components Overview

### Endpoints (Minimal APIs Only)

- `UserQueryEndpoints.cs` - User query operations (minimal APIs)
- `UserCommandEndpoints.cs` - User command operations (minimal APIs)
- `MediatorAnalysisEndpoints.cs` - **Statistics and analysis (minimal APIs)**

### **?? Statistics & Tracking Services**

- `MediatorStatisticsTracker.cs` - **Real-time statistics tracking service**
- `StatisticsCleanupService.cs` - **Background cleanup service**
- `SessionTrackingMiddleware.cs` - **Session-based tracking middleware**
- `StatisticsTrackingMiddleware.cs` - **Mediator request tracking**

### Handlers

**Command Handlers**: 7 handlers for all write operations
**Query Handlers**: 4 handlers for all read operations

### Validators

FluentValidation rules for all command inputs with comprehensive validation.

## ????? Running the Example

### Prerequisites

- .NET 9.0 or later
- Terminal/Command Prompt or Visual Studio

### Steps

1. Navigate to the project directory:
   ```bash
   cd src/samples/UserManagement.Api
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. Open your browser:
   - **Swagger UI**: `https://localhost:5001/swagger` or `http://localhost:5000/swagger`
   - **API Base**: `https://localhost:5001/api` or `http://localhost:5000/api`

## ?? API Endpoints

### User Management Endpoints

#### Query Endpoints
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/users/{id}` | Get user by ID |
| `GET` | `/api/users` | Get paginated users (with search/filter) |
| `GET` | `/api/users/active` | Get active users only |
| `GET` | `/api/users/{id}/statistics` | Get user statistics |

**Query Parameters for `/api/users`:**
- `page` (int, default: 1) - Page number
- `pageSize` (int, default: 10) - Items per page
- `searchTerm` (string, optional) - Search in name or email
- `includeInactive` (bool, default: false) - Include inactive users

#### Command Endpoints
| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/users` | Create new user |
| `POST` | `/api/users/with-id` | Create user with specific ID |
| `PUT` | `/api/users/{id}` | Update user |
| `PUT` | `/api/users/{id}/with-result` | Update user with result |
| `POST` | `/api/users/{id}/activate` | Activate user account |
| `POST` | `/api/users/{id}/deactivate` | Deactivate user account |
| `DELETE` | `/api/users/{id}` | Delete user |

### **?? Real-Time Statistics Endpoints**

| Method | Endpoint | Description | Status |
|--------|----------|-------------|---------|
| `GET` | `/api/mediator/session` | **Get current session ID** | ? **Working** |
| `GET` | `/api/mediator/statistics` | **Real-time global statistics** | ? **Working** |
| `GET` | `/api/mediator/statistics/session/{id}` | **Session-specific statistics** | ? **Working** |
| `GET` | `/api/mediator/statistics/sessions` | **All active sessions** | ? **Working** |

### **?? Analysis & Health Endpoints**

| Method | Endpoint | Description | Status |
|--------|----------|-------------|---------|
| `GET` | `/api/analysis/health` | **Health status** | ? **Working** |

## ?? **Real-Time Statistics Features**

### Working Session Tracking

**? Session ID Endpoint Now Working:**
```bash
GET /api/mediator/session
```

**Sample Response:**
```json
{
  "message": "Current Session ID",
  "sessionId": "stats_1758006198_6cb83558",
  "note": "This session ID is used for tracking your mediator statistics across requests",
  "usage": {
    "viewSessionStats": "GET /api/mediator/statistics/session/stats_1758006198_6cb83558",
    "viewGlobalStats": "GET /api/mediator/statistics",
    "viewAllSessions": "GET /api/mediator/statistics/sessions"
  },
  "sessionInfo": {
    "sessionAvailable": true,
    "aspNetCoreSessionId": "...",
    "statisticsSessionId": "stats_1758006198_6cb83558",
    "sessionKeys": ["MediatorStatisticsSessionId"]
  }
}
```

### Global Statistics

**? Real-Time Global Statistics:**
```bash
GET /api/mediator/statistics
```

**Response includes:**
- Query execution counts by type
- Command execution counts by type
- Active session count
- Real-time updates as requests are processed
- Session tracking information

### Session-Specific Statistics

**? Per-Session Statistics:**
```bash
GET /api/mediator/statistics/session/{sessionId}
```

Track individual user/session activity with detailed breakdowns.

## ?? Session Tracking

### How It Works

1. **Session Creation**: `SessionTrackingMiddleware` creates unique session IDs
2. **Request Tracking**: `StatisticsTrackingMiddleware` tracks all mediator requests
3. **Session Persistence**: Session IDs stored in ASP.NET Core session state
4. **Real-Time Updates**: Statistics update immediately as requests are processed
5. **Automatic Cleanup**: Background service cleans up inactive sessions

### Session ID Format

Session IDs follow the pattern: `stats_{timestamp}_{randomId}`
- Example: `stats_1758006198_6cb83558`

## ?? **Unique Features**

### ?? **Pure Minimal API Architecture**
- **No Controllers**: 100% minimal API implementation
- **Modern Pattern**: Latest ASP.NET Core endpoint patterns
- **Consistent Design**: Single approach throughout the application

### ?? **Complete Real-Time Statistics System**
- **? Working Session Tracking**: Full session-based statistics
- **? Live Updates**: Real-time statistics that update with each request
- **? Global & Session Metrics**: Both application-wide and per-user tracking
- **? Automatic Management**: Background cleanup and memory management

### ?? **Dual Middleware Approach**
- **Mediator Middleware**: Statistics tracking within the mediator pipeline
- **ASP.NET Middleware**: Session management in the HTTP pipeline
- **Seamless Integration**: Both work together transparently

### ??? **Developer Experience**
- **Auto-Registration**: Zero configuration setup
- **Comprehensive Logging**: Detailed request/response logging
- **Error Handling**: Structured exception handling
- **Type Safety**: Full type safety throughout

## ?? Key Learnings

This example demonstrates:

- **Pure Minimal API Development** - Modern ASP.NET Core endpoint patterns
- **Real-Time Statistics Implementation** - Live monitoring and tracking systems
- **Session Management** - Per-user session tracking and persistence
- **CQRS Pattern** - Clean separation of read/write operations
- **Auto-Registration Power** - Automatic discovery eliminates boilerplate
- **Middleware Integration** - Both mediator and ASP.NET Core middleware
- **Clean Architecture** - Well-organized, maintainable structure
- **Modern .NET Patterns** - Latest .NET 9 features and practices

## ? **Test Coverage**

**Test Results**: ? **20/20 tests passing** (100% success rate)

- Comprehensive integration tests for all endpoints
- Validation testing for all command inputs
- Error handling verification
- Session tracking functionality tests
- Statistics endpoint testing

**Test Categories:**
- User query operations (4 tests)
- User command operations (10 tests)
- Validation scenarios (3 tests)
- Error handling (3 tests)

## ?? Technologies Used

- **.NET 9.0** - Latest .NET framework
- **Blazing.Mediator** - CQRS and mediator pattern with statistics
- **ASP.NET Core Minimal APIs** - Modern endpoint definition (NO controllers)
- **Entity Framework Core** - In-memory database for demo
- **FluentValidation** - Declarative validation
- **ASP.NET Core Session State** - Session management
- **Background Services** - `IHostedService` for cleanup
- **Swagger/OpenAPI** - API documentation
- **xUnit & Shouldly** - Comprehensive testing

## ?? Further Reading

- [Blazing.Mediator Documentation](../../docs/)
- [CQRS Pattern Guide](../../docs/MEDIATOR_PATTERN_GUIDE.md)
- [Minimal APIs in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [Real-Time Statistics Implementation Guide](./docs/STATISTICS_GUIDE.md)
- [Session Management in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/app-state)

---

## ?? **Comparison with ECommerce.Api**

| Feature | UserManagement.Api | ECommerce.Api |
|---------|-------------------|---------------|
| **API Pattern** | ?? Pure Minimal APIs | ??? Traditional Controllers + Minimal APIs |
| **Statistics** | ? Full implementation | ? Full implementation |
| **Session Tracking** | ? Working | ? Working |
| **Test Coverage** | ? 20/20 passing | ? Working |
| **Architecture** | ?? Consistent minimal API | ?? Mixed approach |

**Choose UserManagement.Api** for modern, consistent minimal API patterns.
**Choose ECommerce.Api** for traditional controller-based development.