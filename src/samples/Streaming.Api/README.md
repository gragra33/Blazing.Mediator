# Blazing.Mediator Streaming Sample

This sample demonstrates the comprehensive streaming capabilities of **Blazing.Mediator** with real-time JSON data processing. Experience how **Blazing.Mediator** powers multiple streaming implementations across 6 different examples, showcasing various patterns including minimal APIs, Blazor SSR, Auto Mode, Static SSR, and WebAssembly streaming.

## What This Sample Demonstrates

The **Streaming.Api** sample projects showcase the power and flexibility of **Blazing.Mediator** streaming with multiple implementation examples:

### 1. **Minimal API Streaming**

RESTful API endpoints with JSON streaming and Server-Sent Events (SSE), perfect for mobile apps, web clients, and service-to-service communication.

-   JSON Array Streaming (`/api/contacts/stream`)
-   Server-Sent Events with real-time metadata
-   Search filtering and performance metrics
-   RESTful design patterns
-   Interactive Swagger documentation for testing at `https://localhost:7021/swagger`.

### 2. **Blazor SSR Streaming** (`/streaming-ssr`)

Server-Side Rendered Blazor with real-time streaming capabilities, demonstrating responsive server-rendered applications.

-   Real-time server-side updates
-   Interactive streaming controls
-   Live statistics and performance metrics
-   Responsive table layout with search

### 3. **Blazor Auto Mode Streaming** (`/streaming/auto`)

Hybrid approach that starts with Server mode for fast initial load, then automatically upgrades to WebAssembly when downloaded. We chose this hybrid approach over full static render because we wanted **live streaming capabilities** - static mode doesn't support real-time streaming interactions.

-   Fast initial server-side load
-   Automatic WebAssembly upgrade
-   Best of both worlds: speed + interactivity
-   Live streaming support (not available in static mode)

### 4. **Blazor Static SSR Streaming** (`/streaming-static`)

Pure server-side rendering with no JavaScript dependencies, optimised for maximum compatibility and SEO.

-   No JavaScript required
-   Maximum browser compatibility
-   SEO-optimised streaming
-   Accessibility-focused design

### 5. **Blazor Interactive Streaming (WebAssembly)** (`https://localhost:5011/streaming-interactive`)

WebAssembly-powered client-side streaming using EventSource for real-time data flow.

-   Client-side WebAssembly performance
-   EventSource integration for real-time updates
-   Advanced UI features and interaction patterns
-   Multiple streaming modes

### 6. **Blazor Non-Streaming (WebAssembly)** (`https://localhost:5011/non-streaming`)

Traditional bulk data loading for performance comparison with streaming approaches.

-   Classic REST API patterns
-   Bulk JSON loading comparison
-   Performance benchmarking
-   Load time metrics vs streaming

> **Important Note**: The **Streaming.Api** server must be running for the **Streaming.Api.WASM** project to work properly with live data.

## Key Technologies & Features

-   **.NET 9**: Latest framework with enhanced streaming capabilities
-   **Blazing.Mediator**: Lightweight CQRS mediator with streaming support
-   **Multiple Render Modes**: SSR, Auto, Static, and WebAssembly
-   **Real-time Streaming**: Server-Sent Events and JSON streaming
-   **Performance Optimised**: Memory-efficient `IAsyncEnumerable<T>` processing
-   **Comprehensive Examples**: 6 different streaming implementations

## Quick Start

### Prerequisites

-   .NET 9 SDK
-   Visual Studio 2022 or VS Code

### Running the Sample

```bash
cd c:\Blazing.Mediator\src\samples\Streaming.Api\Streaming.Api
dotnet run
```

### Access Points

-   **Main Server**: https://localhost:7021 (Streaming.Api)
-   **WebAssembly Client**: https://localhost:5011 (Streaming.Api.WASM)
-   **API Documentation**: https://localhost:7021/swagger
-   **Sample Endpoints**:
    -   `/api/contacts/count` - Get contact count
    -   `/api/contacts/stream` - JSON streaming
    -   `/api/contacts/stream/sse` - Server-Sent Events

## Project Structure

The sample includes multiple projects demonstrating different aspects:

-   **Streaming.Api**: Main server with minimal APIs and Blazor SSR
-   **Streaming.Api.WASM**: WebAssembly client demonstrating client-side streaming
-   **Streaming.Api.Client**: Shared Razor components
-   **Streaming.Api.Shared**: Common models and DTOs

Each project showcases **Blazing.Mediator** streaming capabilities in different scenarios, from simple API endpoints to complex interactive UIs.
