using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.EventHandlers;
using PureDelivery.Shared.Contracts.Events.Payments;

namespace OrderService.Application.Consumers;

/// <summary>
/// Consumes <see cref="OrderPaidEvent"/> published by PaymentService via RabbitMQ.
/// Persists the order to the database and clears the session cart entry.
/// </summary>
public class OrderPaidConsumer(
    PaymentCompletedEventHandler handler,
    ILogger<OrderPaidConsumer> logger) : IConsumer<OrderPaidEvent>
{
    public async Task Consume(ConsumeContext<OrderPaidEvent> context)
    {
        var evt = context.Message;

        logger.LogInformation(
            "OrderPaidEvent received — session {SessionId}, restaurant {RestaurantId}, payment {PaymentId}, amount {Amount:C}",
            evt.SessionId, evt.RestaurantId, evt.PaymentId, evt.Amount);

        await handler.HandlePaymentCompletedAsync(
            evt.SessionId,
            evt.RestaurantId,
            evt.PaymentId,
            context.CancellationToken);
    }
}
