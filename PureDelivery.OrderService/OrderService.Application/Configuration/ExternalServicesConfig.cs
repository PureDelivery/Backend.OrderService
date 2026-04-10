namespace OrderService.Application.Configuration;

public class ExternalServicesConfig
{
    public RestaurantServiceSettings RestaurantService { get; set; } = new();
    public CatalogServiceSettings CatalogService { get; set; } = new();
}

public class RestaurantServiceSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string GetDetailsEndpoint { get; set; } = string.Empty; // api/v1/restaurant/Restaurant
}

public class CatalogServiceSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string GetBulkItemsEndpoint { get; set; } = string.Empty; // api/v1/restaurant/Menu/items/bulk
}