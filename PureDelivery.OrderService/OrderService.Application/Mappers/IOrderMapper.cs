using OrderService.Application.DTOs.Requests;
using OrderService.Application.DTOs.Responses;
using OrderService.Domain.Entities;

namespace OrderService.Application.Mappers;

public interface IOrderMapper
{
    OrderDto MapToDto(Order order);
    Order MapFromCreateRequest(CreateOrderRequest request);
}

