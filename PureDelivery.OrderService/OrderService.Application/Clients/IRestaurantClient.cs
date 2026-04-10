using PureDelivery.Shared.Contracts.DTOs.Restaurants.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Clients
{
    public interface IRestaurantClient
    {
        Task<RestaurantDetailDto> GetRestaurantAsync(string restaurantId, CancellationToken ct = default);
    }
}
