using Microsoft.Extensions.Logging;
using OrderService.Application.Clients; // ����� ���� IRestaurantClient � ICatalogClient
using OrderService.Application.Exceptions;
using OrderService.Application.Integration;
using OrderService.Application.Repositories;
using OrderService.Domain.Entities;
using PureDelivery.Shared.Contracts.Common.Services;
using PureDelivery.Shared.Contracts.Domain.Enums;
using PureDelivery.Shared.Contracts.DTOs.Restaurants.Responses;
using PureDelivery.Shared.Contracts.DTOs.Session;
using PureDelivery.Shared.Contracts.DTOs.SessionDTO;
using PureDelivery.Shared.Contracts.Events.Orders;

namespace OrderService.Application.Services.impl;

public class OrderSessionService : IOrderSessionService
{
    private readonly ISessionService _sessionService;
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderIntegrationEventPublisher _integrationEvents;
    private readonly IRestaurantClient _restaurantClient;
    private readonly ICatalogClient _catalogClient;
    private readonly ILogger<OrderSessionService> _logger;

    public OrderSessionService(
        ISessionService sessionService,
        IOrderRepository orderRepository,
        IOrderIntegrationEventPublisher integrationEvents,
        IRestaurantClient restaurantClient,
        ICatalogClient catalogClient,
        ILogger<OrderSessionService> logger)
    {
        _sessionService = sessionService;
        _orderRepository = orderRepository;
        _integrationEvents = integrationEvents;
        _restaurantClient = restaurantClient;
        _catalogClient = catalogClient;
        _logger = logger;
    }

    #region Session Management (Redis)

    public async Task<PureDelivery.Shared.Contracts.DTOs.SessionDTO.SessionDto> UpdateOrderStateInSessionAsync(string sessionId, OrderStateDto orderState, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating session {SessionId} for restaurant {RestaurantId}", sessionId, orderState.RestaurantId);

        var session = await _sessionService.GetSessionAsync(sessionId) 
                      ?? throw new OrderException("SESSION_NOT_FOUND", "Session not found", 404);

        if (string.IsNullOrEmpty(orderState.RestaurantId))
            throw new OrderException("ORDER_INVALID", "Restaurant ID is required", 400);

        var now = DateTime.UtcNow;
        session.OrderStates ??= new Dictionary<string, OrderStateDto>();

        if (orderState.Items == null || !orderState.Items.Any())
        {
            session.OrderStates.Remove(orderState.RestaurantId);
            session.LastAccessedAt = now;
            if (!await _sessionService.SaveSessionAsync(session))
                throw new OrderException("SESSION_SAVE_FAILED", "Failed to persist session", 500);
            return session;
        }

        orderState.CreatedAt = session.OrderStates.TryGetValue(orderState.RestaurantId, out var existing)
            ? existing.CreatedAt : now;
        orderState.LastUpdated = now;

        session.OrderStates[orderState.RestaurantId] = orderState;
        session.LastAccessedAt = now;

        if (!await _sessionService.SaveSessionAsync(session))
            throw new OrderException("SESSION_SAVE_FAILED", "Failed to persist session", 500);

        return session;
    }

    public async Task<OrderStateDto?> GetOrderStateFromSessionAsync(string sessionId, string? restaurantId = null, CancellationToken ct = default)
    {
        var session = await _sessionService.GetSessionAsync(sessionId);
        if (session == null) return null;

        if (!string.IsNullOrEmpty(restaurantId))
            return session.OrderStates.TryGetValue(restaurantId, out var state) ? state : null;

        return session.OrderStates.Values.OrderByDescending(x => x.LastUpdated).FirstOrDefault();
    }

    public async Task<PureDelivery.Shared.Contracts.DTOs.SessionDTO.SessionDto?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return await _sessionService.GetSessionAsync(sessionId);
    }

    #endregion

    #region Order Creation (SQL Save)

    public async Task<Guid> SaveOrderFromSessionAsync(string sessionId, string restaurantId, Guid? paymentId, decimal paidAmount, int paymentMethodCode, CancellationToken ct = default)
    {
        try
        {
            var session = await _sessionService.GetSessionAsync(sessionId)
                          ?? throw new OrderException("SESSION_NOT_FOUND", "Session not found", 404);

            if (!session.OrderStates.TryGetValue(restaurantId, out var stateDto))
                throw new OrderException("ORDER_INVALID", "Cart not found", 400);

            var menuItemIds = stateDto.Items.Select(i => i.Id).ToList();
            var restaurantTask = _restaurantClient.GetRestaurantAsync(restaurantId, ct);
            var menuItemsTask = _catalogClient.GetMenuItemsAsync(menuItemIds, ct);
            await Task.WhenAll(restaurantTask, menuItemsTask);
            var restaurantInfo = await restaurantTask;
            var menuItemsInfo = await menuItemsTask;

            var order = MapToEntity(stateDto, session, restaurantInfo, menuItemsInfo, paymentId, paidAmount, paymentMethodCode);

            var savedOrder = await _orderRepository.CreateAsync(order, ct);

            session.OrderStates.Remove(restaurantId);
            await _sessionService.SaveSessionAsync(session);

            await PublishIntegrationEvents(savedOrder, session, sessionId, ct);

            return savedOrder.Id;
        }
        catch (Exception ex) when (ex is not OrderException)
        {
            _logger.LogError(ex, "Failed to save order for session {SessionId}", sessionId);
            throw new OrderException("ORDER_SAVE_FAILED", "Internal error during order save", ex, 500);
        }
    }

    private Order MapToEntity(
        OrderStateDto state,
        PureDelivery.Shared.Contracts.DTOs.SessionDTO.SessionDto session,
        RestaurantDetailDto restInfo,
        List<MenuItemDetailDto> menuInfo,
        Guid? paymentId,
        decimal paidAmount,
        int paymentMethodCode)
    {
        var now = DateTime.UtcNow;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = session.CustomerSessionDto?.Id ?? Guid.Empty,
            CustomerEmail = session.CustomerSessionDto?.Email ?? string.Empty,
            CustomerName = session.CustomerSessionDto?.FullName ?? string.Empty,
            RestaurantId = restInfo.Id,
            RestaurantName = restInfo.Name,
            Status = OrderStatus.Confirmed,
            CreatedAt = now,
            SessionId = session.SessionId,
            PaymentId = paymentId,
            PaymentMethod = (PaymentMethod)paymentMethodCode,
            PaymentStatus = paymentId.HasValue ? PaymentStatus.Completed : PaymentStatus.Pending,
            
            DeliveryAddress = new AddressSnapshot
            {
                AddressId = state.Delivery?.DeliveryAddress?.Id ?? Guid.Empty,
                FullAddressString = state.Delivery?.DeliveryAddress?.FullAddress ?? "Unknown",
                City = state.Delivery?.DeliveryAddress?.City ?? string.Empty,
                Building = state.Delivery?.DeliveryAddress?.Building ?? string.Empty,
                Apartment = state.Delivery?.DeliveryAddress?.Apartment ?? string.Empty,
                Floor = state.Delivery?.DeliveryAddress?.Floor ?? string.Empty,
                Latitude = (decimal)(state.Delivery?.DeliveryAddress?.Latitude ?? 0),
                Longitude = (decimal)(state.Delivery?.DeliveryAddress?.Longitude ?? 0)
            }
        };

        foreach (var itemSession in state.Items)
        {
            var catalogItem = menuInfo.FirstOrDefault(m => m.Id == Guid.Parse(itemSession.Id));
            if (catalogItem == null) continue;

            var orderItem = new OrderItem
            {
                MenuItemId = Guid.Parse(itemSession.Id),
                MenuItemName = catalogItem.Name,
                MenuItemImageUrl = catalogItem.ImageUrl,
                UnitPrice = catalogItem.Price,
                Quantity = itemSession.Quantity,
                SpecialInstructions = itemSession.SpecialInstructions,
                
                SelectedOptions = itemSession.SelectedOptions.Select(opt => 
                {
                    var catalogOpt = catalogItem.Options.FirstOrDefault(o => o.Id.ToString() == opt.OptionId);
                    var catalogChoice = catalogOpt?.Choices.FirstOrDefault(c => c.Id.ToString() == opt.ChoiceId);

                    return new OrderItemOption
                    {
                        OptionId = Guid.Parse(opt.OptionId),
                        OptionName = catalogOpt?.Name ?? "Option",
                        ChoiceId = Guid.Parse(opt.ChoiceId),
                        ChoiceName = catalogChoice?.Name ?? "Choice",
                        AdditionalPrice = catalogChoice?.PriceModifier ?? 0
                    };
                }).ToList()
            };

            orderItem.TotalPrice = (orderItem.UnitPrice + orderItem.SelectedOptions.Sum(o => o.AdditionalPrice)) * orderItem.Quantity ?? 0;
            order.Items.Add(orderItem);
        }

        // ������� ����� (MONEY VALUE OBJECT)
        order.Money = new OrderMoney
        {
            SubTotal = order.Items.Sum(i => i.TotalPrice),
            DeliveryFee = state.Payment?.DeliveryFee ?? 0,
            Discount = state.Payment?.Discount ?? 0,
            Tax = 0 // ������ ���� ������ ������� ������
        };
        order.Money.TotalAmount = order.Money.SubTotal + order.Money.DeliveryFee - order.Money.Discount;

        // ������ ������ � �������
        order.History.Add(new OrderHistory
        {
            Status = OrderStatus.Confirmed,
            ChangedAt = now,
            ChangedBy = "System",
            Comment = "Order initialized from session."
        });

        return order;
    }

    private async Task PublishIntegrationEvents(Order savedOrder, PureDelivery.Shared.Contracts.DTOs.SessionDTO.SessionDto session, string sessionId, CancellationToken ct)
    {
        await _integrationEvents.PublishOrderCreatedAsync(new OrderCreatedEvent
        {
            OrderId = savedOrder.Id.ToString(),
            UserId = session.UserId,
            CustomerEmail = session.CustomerSessionDto?.Email ?? string.Empty,
            CustomerName = session.CustomerSessionDto?.FullName ?? string.Empty,
            RestaurantId = savedOrder.RestaurantId.ToString(),
            RestaurantName = savedOrder.RestaurantName,
            TotalAmount = savedOrder.Money.TotalAmount,
            CreatedAt = savedOrder.CreatedAt,
            SessionId = sessionId
        }, ct);
    }

    #endregion
}