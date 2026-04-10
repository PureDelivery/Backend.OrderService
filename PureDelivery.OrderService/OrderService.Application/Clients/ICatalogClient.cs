using PureDelivery.Shared.Contracts.DTOs.Restaurants.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Clients
{
    public interface ICatalogClient
    {
        Task<List<MenuItemDetailDto>> GetMenuItemsAsync(List<string> ids, CancellationToken ct = default);
    }
}
