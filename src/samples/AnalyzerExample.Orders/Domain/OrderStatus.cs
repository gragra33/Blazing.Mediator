namespace AnalyzerExample.Orders.Domain;

public enum OrderStatus
{
    Pending,
    PaymentPending,
    Confirmed,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    Refunded,
    Returned
}