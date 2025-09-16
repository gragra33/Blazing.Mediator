namespace MiddlewareExample.Handlers;

/// <summary>
/// Handles <see cref="UpdateCustomerDetailsCommand"/> requests for updating customer information.
/// </summary>
public class UpdateCustomerDetailsCommandHandler(ILogger<UpdateCustomerDetailsCommandHandler> logger)
    : IRequestHandler<UpdateCustomerDetailsCommand, bool>
{
    /// <inheritdoc />
    public Task<bool> Handle(UpdateCustomerDetailsCommand request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(".. Updating customer details for ID: {CustomerId}", request.CustomerId);
        logger.LogInformation("-- Name: {FullName}, Email: {Email}, Contact Method: {ContactMethod}",
            request.FullName, request.Email, request.ContactMethod);

        // Simulate customer update operation
        // In a real application, this would update the database
        var isSuccess = true; // Simulate success

        if (isSuccess)
        {
            logger.LogInformation("-- Customer details updated successfully for {CustomerId}", request.CustomerId);
        }
        else
        {
            logger.LogWarning("-- Failed to update customer details for {CustomerId}", request.CustomerId);
        }

        return Task.FromResult(isSuccess);
    }
}
