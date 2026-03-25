namespace Nexus.Application.Dto.Suppliers;

public record SupplierDto(
    long Id,
    long CompanyId,
    string Name,
    string TaxId,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
