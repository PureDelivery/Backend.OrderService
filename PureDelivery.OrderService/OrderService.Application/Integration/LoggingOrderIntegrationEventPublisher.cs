using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using PureDelivery.Shared.Contracts.Events.Orders;

namespace OrderService.Application.Integration;

public class LoggingOrderIntegrationEventPublisher : IOrderIntegrationEventPublisher
{
    private readonly ILogger<LoggingOrderIntegrationEventPublisher> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public LoggingOrderIntegrationEventPublisher(ILogger<LoggingOrderIntegrationEventPublisher> logger, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishOrderCreatedAsync(OrderCreatedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Integration event OrderCreated: {Payload}",
            JsonSerializer.Serialize(integrationEvent, JsonOptions));
        
        await _publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
