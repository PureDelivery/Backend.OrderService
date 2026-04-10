using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.Requests;
using OrderService.Application.DTOs.Responses;
using OrderService.Application.Exceptions;
using OrderService.Application.Mappers;
using OrderService.Application.Repositories;
using OrderService.Domain.Entities;
using PureDelivery.Shared.Contracts.Common;
using PureDelivery.Shared.Contracts.Domain.Enums;
using PureDelivery.Shared.Contracts.Domain.Models;

namespace OrderService.Application.Services.impl;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderMapper _mapper;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IOrderMapper mapper,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _logger = logger;
    }

    // ВНУТРЕННИЙ МЕТОД: Используется только OrderSessionService для сохранения в базу
    public async Task<Order> SaveNewOrderAsync(Order order, CancellationToken ct)
    {
        _logger.LogInformation("Saving new order {OrderId} to database", order.Id);

        // Инициализируем историю
        order.History.Add(new OrderHistory
        {
            Status = OrderStatus.Confirmed,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = "System",
            Comment = "Заказ успешно создан и зафиксирован в БД."
        });

        return await _orderRepository.CreateAsync(order, ct);
    }

    public async Task<BaseResponse<OrderDto>> GetOrderByIdAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(orderId, ct);
        if (order == null)
            throw new OrderException("ORDER_NOT_FOUND", $"Заказ {orderId} не найден", 404);

        return BaseResponse<OrderDto>.Success(_mapper.MapToDto(order));
    }

    public async Task<BaseResponse<PagedResult<OrderDto>>> GetCustomerOrdersAsync(Guid customerId, int page, int pageSize, CancellationToken ct)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(customerId, ct);
        var list = orders.ToList();

        var paged = list.Skip((page - 1) * pageSize).Take(pageSize).Select(_mapper.MapToDto).ToList();

        return BaseResponse<PagedResult<OrderDto>>.Success(new PagedResult<OrderDto>
        {
            Items = paged,
            TotalCount = list.Count,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<BaseResponse<OrderDto>> UpdateStatusAsync(Guid orderId, OrderStatus newStatus, string comment, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct);
        if (order == null) throw new OrderException("NOT_FOUND", "Заказ не найден", 404);

        _logger.LogInformation("Updating status for {OrderId}: {Old} -> {New}", orderId, order.Status, newStatus);

        order.Status = newStatus;
        order.History.Add(new OrderHistory
        {
            Status = newStatus,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = "Operator", // В будущем подтянем из контекста
            Comment = comment
        });

        await _orderRepository.UpdateAsync(order, ct);
        return BaseResponse<OrderDto>.Success(_mapper.MapToDto(order));
    }
}