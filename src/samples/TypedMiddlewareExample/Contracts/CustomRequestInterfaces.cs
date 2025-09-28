namespace TypedMiddlewareExample.Contracts;

/// <summary>
/// Custom interface for product-related requests to demonstrate type constraints.
/// This shows how to create domain-specific request interfaces.
/// </summary>
public interface IProductRequest : IRequest
{
}

/// <summary>
/// Custom interface for product-related requests with responses to demonstrate type constraints.
/// </summary>
/// <typeparam name="TResponse">The response type</typeparam>
public interface IProductRequest<TResponse> : IRequest<TResponse>
{
}

/// <summary>
/// Custom interface for customer-related requests to demonstrate type constraints.
/// This shows how to create domain-specific request interfaces.
/// </summary>
public interface ICustomerRequest : IRequest
{
}

/// <summary>
/// Custom interface for customer-related requests with responses to demonstrate type constraints.
/// </summary>
/// <typeparam name="TResponse">The response type</typeparam>
public interface ICustomerRequest<TResponse> : IRequest<TResponse>
{
}

/// <summary>
/// Custom interface for inventory-related requests to demonstrate type constraints.
/// </summary>
/// <typeparam name="TResponse">The response type</typeparam>
public interface IInventoryRequest<TResponse> : IRequest<TResponse>
{
}

/// <summary>
/// Custom interface for order-related requests to demonstrate type constraints.
/// </summary>
public interface IOrderRequest : IRequest
{
}