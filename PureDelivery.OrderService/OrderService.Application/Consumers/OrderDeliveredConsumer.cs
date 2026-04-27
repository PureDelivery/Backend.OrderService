using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Services;
using PureDelivery.Shared.Contracts.Domain.Enums;
using PureDelivery.Shared.Contracts.Events.Orders;

namespace OrderService.Application.Consumers;

public class OrderDeliveredConsumer(
    IOrderService orderService,
    ILogger<OrderDeliveredConsumer> logger) : IConsumer<OrderDeliveredEvent>
{
    public async Task Consume(ConsumeContext<OrderDeliveredEvent> context)
    {
        var e = context.Message;
        logger.LogInformation("OrderDelivered for order {OrderId} by courier {CourierId}", e.OrderId, e.CourierId);

        if (!Guid.TryParse(e.OrderId, out var orderId)) return;

        await orderService.UpdateStatusAsync(
            orderId,
            OrderStatus.Completed,
            "CourierService",
            "Order delivered to customer",
            context.CancellationToken);
    }
}
