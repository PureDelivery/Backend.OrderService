using MassTransit;
using Microsoft.Extensions.Logging;
using PureDelivery.Shared.Contracts.Events.Orders;

namespace OrderService.Application.Consumers;

/// <summary>
/// Courier accepted the order — we just log it.
/// Order status does NOT advance here; it moves to Delivery only when
/// the restaurant confirms courier pickup via OrderPickedUpEvent.
/// </summary>
public class CourierAssignedConsumer(
    ILogger<CourierAssignedConsumer> logger) : IConsumer<CourierAssignedEvent>
{
    public Task Consume(ConsumeContext<CourierAssignedEvent> context)
    {
        var e = context.Message;
        logger.LogInformation("Courier {CourierId} assigned to order {OrderId} — awaiting restaurant pickup confirmation",
            e.CourierId, e.OrderId);
        return Task.CompletedTask;
    }
}
