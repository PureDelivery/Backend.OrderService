namespace OrderService.Domain.Entities;

public class AddressSnapshot
{
    public Guid AddressId { get; set; }
    public string FullAddressString { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;
    public string Apartment { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}