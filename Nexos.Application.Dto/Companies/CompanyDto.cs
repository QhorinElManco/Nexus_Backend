namespace Nexos.Application.Dto.Companies;

public record CompanyDto(
    long Id,
    string Name,
    string TaxId,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
