using OrderService.Application.DTOs.Requests;
using OrderService.Application.DTOs.Responses;
using OrderService.Domain.Entities;
using PureDelivery.Shared.Contracts.Domain.Enums;

namespace OrderService.Application.Mappers;

public class OrderMapper : IOrderMapper
{
    public OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            RestaurantId = order.RestaurantId,
            RestaurantName = order.RestaurantName,
            SessionId = order.SessionId,
            PaymentId = order.PaymentId,
            Status = order.Status,
            PaymentStatus = order.PaymentStatus,
            CreatedAt = order.CreatedAt,

            // Мапим из вложенного объекта Money
            SubTotal = order.Money.SubTotal,
            DeliveryFee = order.Money.DeliveryFee,
            Tax = order.Money.Tax,
            Discount = order.Money.Discount,
            TotalAmount = order.Money.TotalAmount,

            // Мапим из вложенного объекта DeliveryAddress (AddressSnapshot)
            DeliveryAddress = order.DeliveryAddress.FullAddressString,
            DeliveryLatitude = order.DeliveryAddress.Latitude,
            DeliveryLongitude = order.DeliveryAddress.Longitude,

            Items = order.Items.Select(MapItemToDto).ToList()
            // Убрал CustomerName, Email и прочее, так как в твоей новой модели Order их нет
        };
    }

    private OrderItemDto MapItemToDto(OrderItem item)
    {
        return new OrderItemDto
        {
            Id = item.Id,
            MenuItemId = item.MenuItemId,
            MenuItemName = item.MenuItemName,
            MenuItemImageUrl = item.MenuItemImageUrl,
            UnitPrice = item.UnitPrice,
            Quantity = item.Quantity,
            TotalPrice = item.TotalPrice,
            SpecialInstructions = item.SpecialInstructions,
            SelectedOptions = item.SelectedOptions.Select(MapOptionToDto).ToList()
        };
    }

    private OrderItemOptionDto MapOptionToDto(OrderItemOption option)
    {
        return new OrderItemOptionDto
        {
            Id = option.Id,
            OptionId = option.OptionId,
            OptionName = option.OptionName,
            ChoiceId = option.ChoiceId,
            ChoiceName = option.ChoiceName,
            AdditionalPrice = option.AdditionalPrice ?? 0
        };
    }

    // Этот метод теперь почти не нужен, так как мы создаем заказ через OrderSessionService
    // Но если оставляешь, мапь только то, что есть в модели
    public Order MapFromCreateRequest(CreateOrderRequest request)
    {
        return new Order
        {
            CustomerId = request.CustomerId,
            RestaurantId = request.RestaurantId,
            SessionId = request.SessionId,
            Status = OrderStatus.Cart,
            CreatedAt = DateTime.UtcNow,
            Items = request.Items.Select(MapItemFromRequest).ToList()
        };
    }

    private OrderItem MapItemFromRequest(OrderItemRequest request)
    {
        return new OrderItem
        {
            MenuItemId = request.MenuItemId,
            Quantity = request.Quantity,
            SpecialInstructions = request.SpecialInstructions,
            SelectedOptions = request.SelectedOptions?.Select(MapOptionFromRequest).ToList() ?? new List<OrderItemOption>()
        };
    }

    private OrderItemOption MapOptionFromRequest(OrderItemOptionRequest request)
    {
        return new OrderItemOption
        {
            OptionId = request.OptionId,
            ChoiceId = request.ChoiceId
        };
    }
}