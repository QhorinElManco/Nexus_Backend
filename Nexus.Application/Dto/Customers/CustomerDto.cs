namespace Nexus.Application.Dto.Customers;

public record CustomerDto(
    long Id,
    long CompanyId,
    string CompanyName,
    string Name,
    string? TradeName,
    string TaxId,
    double? Lat,
    double? Lng,
    string Status,
    IReadOnlyList<CustomerAssignmentDto> Assignments,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CustomerAssignmentDto(
    long Id,
    long CustomerId,
    long UserId,
    string UserFullName,
    int DayOfWeek,
    int SequenceOrder
);
