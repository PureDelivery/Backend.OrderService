using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.Requests;
using OrderService.Application.DTOs.Responses;
using OrderService.Application.Exceptions;
using OrderService.Application.Integration;
using OrderService.Application.Mappers;
using OrderService.Application.Repositories;
using OrderService.Domain.Entities;
using PureDelivery.Shared.Contracts.Common;
using PureDelivery.Shared.Contracts.Domain.Enums;
using PureDelivery.Shared.Contracts.Domain.Models;
using PureDelivery.Shared.Contracts.Events.Orders;

namespace OrderService.Application.Services.impl;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderMapper _mapper;
    private readonly IOrderIntegrationEventPublisher _events;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IOrderMapper mapper,
        IOrderIntegrationEventPublisher events,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _events = events;
        _logger = logger;
    }

    public async Task<Order> SaveNewOrderAsync(Order order, CancellationToken ct)
    {
        _logger.LogInformation("Saving new order {OrderId} to database", order.Id);
        order.History.Add(new OrderHistory
        {
            Status = OrderStatus.Confirmed,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = "System",
            Comment = "Order created after payment confirmed."
        });
        return await _orderRepository.CreateAsync(order, ct);
    }

    public async Task<BaseResponse<OrderDto>> GetOrderByIdAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(orderId, ct);
        if (order == null)
            throw new OrderException("ORDER_NOT_FOUND", $"Order {orderId} not found", 404);

        return BaseResponse<OrderDto>.Success(_mapper.MapToDto(order));
    }

    public async Task<BaseResponse<PagedResult<OrderDto>>> GetCustomerOrdersAsync(Guid customerId, int page, int pageSize, CancellationToken ct)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(customerId, ct);
        var list = orders.OrderByDescending(o => o.CreatedAt).ToList();

        var paged = list.Skip((page - 1) * pageSize).Take(pageSize).Select(_mapper.MapToDto).ToList();

        return BaseResponse<PagedResult<OrderDto>>.Success(new PagedResult<OrderDto>
        {
            Items = paged,
            TotalCount = list.Count,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<BaseResponse<PagedResult<OrderDto>>> GetRestaurantOrdersAsync(Guid restaurantId, int page, int pageSize, CancellationToken ct)
    {
        var orders = await _orderRepository.GetByRestaurantIdAsync(restaurantId, ct);
        var list = orders.OrderByDescending(o => o.CreatedAt).ToList();

        var paged = list.Skip((page - 1) * pageSize).Take(pageSize).Select(_mapper.MapToDto).ToList();

        return BaseResponse<PagedResult<OrderDto>>.Success(new PagedResult<OrderDto>
        {
            Items = paged,
            TotalCount = list.Count,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<BaseResponse<OrderDto>> UpdateStatusAsync(Guid orderId, OrderStatus newStatus, string changedBy, string comment, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct);
        if (order == null) throw new OrderException("NOT_FOUND", "Order not found", 404);

        var oldStatus = order.Status;
        _logger.LogInformation("Updating status for {OrderId}: {Old} -> {New} by {ChangedBy}", orderId, oldStatus, newStatus, changedBy);

        order.Status = newStatus;

        await _orderRepository.AddHistoryAsync(new OrderHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Status = newStatus,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = changedBy,
            Comment = comment
        }, ct);

        await _orderRepository.UpdateAsync(order, ct);

        await _events.PublishOrderStatusChangedAsync(new OrderStatusChangedEvent
        {
            OrderId        = orderId.ToString(),
            CustomerId     = order.CustomerId.ToString(),
            CustomerEmail  = order.CustomerEmail,
            CustomerName   = order.CustomerName,
            RestaurantName = order.RestaurantName,
            OldStatus      = oldStatus,
            NewStatus      = newStatus,
            ChangedAt      = DateTime.UtcNow,
            ChangedBy      = changedBy
        }, ct);

        var updated = await _orderRepository.GetByIdWithItemsAsync(orderId, ct);
        return BaseResponse<OrderDto>.Success(_mapper.MapToDto(updated!));
    }
}
