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
using PureDelivery.Shared.Contracts.Events.Loyalty;
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

    public async Task<Guid> SaveOrderFromSessionAsync(string sessionId, string restaurantId, Guid? paymentId, decimal paidAmount, PaymentMethod paymentMethod, Guid? orderId = null, CancellationToken ct = default)
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

            var order = MapToEntity(stateDto, session, restaurantInfo, menuItemsInfo, paymentId, paidAmount, paymentMethod, orderId);

            var savedOrder = await _orderRepository.CreateAsync(order, ct);

            session.OrderStates.Remove(restaurantId);
            await _sessionService.SaveSessionAsync(session);

            await PublishIntegrationEvents(savedOrder, session, sessionId, restaurantInfo, ct);

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
        PaymentMethod paymentMethod,
        Guid? orderId = null)
    {
        var now = DateTime.UtcNow;

        var order = new Order
        {
            Id = orderId ?? Guid.NewGuid(),
            OrderNumber = Random.Shared.Next(100000, 1000000).ToString(),
            CustomerId = session.CustomerSessionDto?.Id ?? Guid.Empty,
            CustomerEmail = session.CustomerSessionDto?.Email ?? string.Empty,
            CustomerName = session.CustomerSessionDto?.FullName ?? string.Empty,
            RestaurantId = restInfo.Id,
            RestaurantName = restInfo.Name,
            Status = OrderStatus.Confirmed,
            CreatedAt = now,
            SessionId = session.SessionId,
            PaymentId = paymentId,
            PaymentMethod = paymentMethod,
            PaymentStatus = paymentId.HasValue ? PaymentStatus.Completed : PaymentStatus.Pending,
            
            DeliveryAddress = new AddressSnapshot
            {
                AddressId = state.Delivery?.DeliveryAddress?.Id ?? Guid.Empty,
                FullAddressString = state.Delivery?.DeliveryAddress?.FullAddress ?? "Unknown",
                City = state.Delivery?.DeliveryAddress?.City ?? string.Empty,
                Building = state.Delivery?.DeliveryAddress?.Building ?? string.Empty,
                Apartment = state.Delivery?.DeliveryAddress?.Apartment ?? string.Empty,
                Floor = state.Delivery?.DeliveryAddress?.Floor ?? string.Empty,
                Latitude = state.Delivery?.DeliveryAddress?.Latitude ?? 0,
                Longitude = state.Delivery?.DeliveryAddress?.Longitude ?? 0
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

    private async Task PublishIntegrationEvents(Order savedOrder, PureDelivery.Shared.Contracts.DTOs.SessionDTO.SessionDto session, string sessionId, RestaurantDetailDto restaurantInfo, CancellationToken ct)
    {
        if (savedOrder == null) return;

        // Loyalty points: якщо ресторан бере участь у програмі лояльності
        if (restaurantInfo.ParticipatesInLoyalty && savedOrder.CustomerId != Guid.Empty)
        {
            var pointsToAdd = Math.Round(savedOrder.Money.SubTotal * (restaurantInfo.LoyaltyPointsRate / 100), 2);
            if (pointsToAdd > 0)
            {
                await _integrationEvents.PublishLoyaltyEarnedAsync(new LoyaltyEarnedByOrderEvent
                {
                    OrderId = savedOrder.Id,
                    UserId = savedOrder.CustomerId,
                    PointsToAdd = pointsToAdd
                }, ct);
            }
        }

        await _integrationEvents.PublishOrderProcessedAsync(new OrderProcessedEvent
        {
            OrderId = savedOrder.Id.ToString(),
            OrderNumber = savedOrder.OrderNumber,
            SessionId = sessionId,
            CustomerId = savedOrder.CustomerId.ToString(),
            CustomerEmail = session.CustomerSessionDto?.Email ?? string.Empty,
            CustomerName = session.CustomerSessionDto?.FullName ?? string.Empty,
            RestaurantId = savedOrder.RestaurantId.ToString(),
            RestaurantName = savedOrder.RestaurantName,
            TotalAmount = savedOrder.Money.TotalAmount,
            DeliveryFee = savedOrder.Money.DeliveryFee,
            DeliveryLatitude = savedOrder.DeliveryAddress.Latitude,
            DeliveryLongitude = savedOrder.DeliveryAddress.Longitude,
            DeliveryAddress = savedOrder.DeliveryAddress.FullAddressString,
            DeliveryCity = savedOrder.DeliveryAddress.City,
            RestaurantLatitude = restaurantInfo.Address.Latitude,
            RestaurantLongitude = restaurantInfo.Address.Longitude,
            RestaurantAddress = restaurantInfo.Address.FullAddress,
            RestaurantCity = restaurantInfo.Address.City,
            CreatedAt = savedOrder.CreatedAt
        }, ct);
    }

    #endregion
}