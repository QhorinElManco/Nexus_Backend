namespace Nexus.Application.Dto.Customers;

public record CreateCustomerDto(
    long CompanyId,
    string Name,
    string? TradeName,
    string TaxId,
    double? Lat,
    double? Lng,
    string Status
);
