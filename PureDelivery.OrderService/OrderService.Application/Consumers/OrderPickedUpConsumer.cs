using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Services;
using PureDelivery.Shared.Contracts.Domain.Enums;
using PureDelivery.Shared.Contracts.Events.Orders;

namespace OrderService.Application.Consumers;

/// <summary>
/// Restaurant confirmed the pickup (entered courier's code) →
/// advance order status from ReadyForPickup to Delivery.
/// </summary>
public class OrderPickedUpConsumer(
    IOrderService orderService,
    ILogger<OrderPickedUpConsumer> logger) : IConsumer<OrderPickedUpEvent>
{
    public async Task Consume(ConsumeContext<OrderPickedUpEvent> context)
    {
        var e = context.Message;
        logger.LogInformation("Order {OrderId} picked up by courier {CourierId} — setting status to Delivery",
            e.OrderId, e.CourierId);

        if (!Guid.TryParse(e.OrderId, out var orderId)) return;

        await orderService.UpdateStatusAsync(
            orderId,
            OrderStatus.Delivery,
            "CourierService",
            "Courier picked up the order",
            context.CancellationToken);
    }
}
