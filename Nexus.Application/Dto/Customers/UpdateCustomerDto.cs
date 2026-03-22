namespace Nexus.Application.Dto.Customers;

public record UpdateCustomerDto(
    string Name,
    string? TradeName,
    double? Lat,
    double? Lng,
    string Status
);
