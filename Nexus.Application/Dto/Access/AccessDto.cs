namespace Nexus.Application.Dto.Access;

public record AccessDto(
    long Id,
    string Name,
    string? Description,
    IReadOnlyList<RoleSummaryDto> Roles,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record RoleSummaryDto(
    long Id,
    string Name
);
