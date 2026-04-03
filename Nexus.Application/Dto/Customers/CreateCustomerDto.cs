namespace Nexus.Application.Dto.Customers;

public record CreateCustomerDto(
    string Name,
    string? TradeName,
    string TaxId,
    double? Lat,
    double? Lng,
    string Status
);
