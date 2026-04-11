namespace OrderService.Application.Configuration;

public class ExternalServicesConfig
{
    public RestaurantServiceSettings RestaurantService { get; set; } = new();
    public CatalogServiceSettings CatalogService { get; set; } = new();
}
public class RestaurantServiceSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    // Имя должно быть таким же, как в JSON ключе
    public string GetRestaurantEndpoint { get; set; } = string.Empty;
}

public class CatalogServiceSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    // Имя должно быть таким же, как в JSON ключе
    public string GetMenuItemsBulkEndpoint { get; set; } = string.Empty;
}