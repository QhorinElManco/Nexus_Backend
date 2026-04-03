using FluentValidation;
using Microsoft.Extensions.Logging;
using Nexus.Application.Dto.Sales;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Domain.Entities.Sales;

namespace Nexus.Application.UseCases.Sales;

public class VisitService(
    IVisitRepository visitRepository,
    ICustomerRepository customerRepository,
    IUserRepository userRepository,
    IValidator<CreateVisitDto> createValidator,
    IValidator<UpdateVisitDto> updateValidator,
    IValidator<VisitSearchRequest> searchValidator,
    ILogger<VisitService> logger) : IVisitService
{
    private static readonly HashSet<string> ValidStatuses = ["Scheduled", "InProgress", "Completed", "Cancelled"];

    public async Task<Response<VisitDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default)
    {
        var visit = await visitRepository.GetByIdAsync(id, ct);

        if (visit is null || visit.CompanyId != companyId)
        {
            logger.LogWarning("Visit not found [{VisitId}] [{CompanyId}]", id, companyId);
            return Response<VisitDto>.Fail("Visit not found", ErrorCode.NotFound);
        }

        return Response<VisitDto>.Ok(MapToDto(visit));
    }

    public async Task<Response<IReadOnlyList<VisitDto>>> GetByCompanyAsync(long companyId,
        CancellationToken ct = default)
    {
        var visits = await visitRepository.GetByCompanyAsync(companyId, ct);

        var dtos = new List<VisitDto>();
        foreach (var visit in visits)
        {
            var visitWithRelations = await visitRepository.GetByIdAsync(visit.Id, ct);
            if (visitWithRelations != null)
            {
                dtos.Add(MapToDto(visitWithRelations));
            }
        }

        return Response<IReadOnlyList<VisitDto>>.Ok(dtos);
    }

    public async Task<ResponsePagination<VisitDto>> SearchAsync(VisitSearchRequest request, long companyId,
        CancellationToken ct = default)
    {
        var validationResult = await searchValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponsePagination<VisitDto>();
        }

        var (items, totalCount) = await visitRepository.SearchAsync(request, companyId, ct);

        var dtos = new List<VisitDto>();
        foreach (var visit in items)
        {
            var visitWithRelations = await visitRepository.GetByIdAsync(visit.Id, ct);
            if (visitWithRelations != null)
            {
                dtos.Add(MapToDto(visitWithRelations));
            }
        }

        return ResponsePagination<VisitDto>.Ok(dtos, request.Page, request.PageSize, totalCount);
    }

    public async Task<Response<VisitDto>> CreateAsync(CreateVisitDto dto, long companyId, long userId,
        CancellationToken ct = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<VisitDto>();
        }

        // Validate Customer exists and belongs to company
        var customer = await customerRepository.GetByIdAsync(dto.CustomerId, ct);
        if (customer is null || customer.CompanyId != companyId)
        {
            logger.LogWarning("Customer not found [{CustomerId}] [{CompanyId}]", dto.CustomerId, companyId);
            return Response<VisitDto>.Fail("Customer not found", ErrorCode.NotFound);
        }

        // Validate User exists
        var user = await userRepository.GetByIdAsync(userId, ct);
        if (user is null)
        {
            logger.LogWarning("User not found [{UserId}]", userId);
            return Response<VisitDto>.Fail("User not found", ErrorCode.NotFound);
        }

        // Validate Status if provided
        if (dto.Status != null && !ValidStatuses.Contains(dto.Status))
        {
            return Response<VisitDto>.Fail($"Invalid Status. Must be one of: {string.Join(", ", ValidStatuses)}",
                ErrorCode.ValidationError);
        }

        var visit = new Visit
        {
            CompanyId = companyId,
            UserId = userId,
            CustomerId = dto.CustomerId,
            CheckInTime = DateTime.UtcNow,
            CheckInLat = dto.CheckInLat,
            CheckInLng = dto.CheckInLng,
            Status = dto.Status ?? "InProgress"
        };

        var created = await visitRepository.AddAsync(visit, ct);

        logger.LogInformation("Visit created (check-in) [{VisitId}] [{CustomerId}] [{UserId}]",
            created.Id, created.CustomerId, created.UserId);

        var visitWithRelations = await visitRepository.GetByIdAsync(created.Id, ct);
        return Response<VisitDto>.Ok(MapToDto(visitWithRelations!));
    }

    public async Task<Response<VisitDto>> UpdateAsync(long id, UpdateVisitDto dto, long companyId,
        CancellationToken ct = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<VisitDto>();
        }

        var visit = await visitRepository.GetByIdAsync(id, ct);
        if (visit is null || visit.CompanyId != companyId)
        {
            logger.LogWarning("Visit not found [{VisitId}] [{CompanyId}]", id, companyId);
            return Response<VisitDto>.Fail("Visit not found", ErrorCode.NotFound);
        }

        // Validate and apply updates
        if (dto.Status != null)
        {
            if (!ValidStatuses.Contains(dto.Status))
            {
                return Response<VisitDto>.Fail($"Invalid Status. Must be one of: {string.Join(", ", ValidStatuses)}",
                    ErrorCode.ValidationError);
            }

            visit.Status = dto.Status;
        }

        if (dto.CancelReason != null)
        {
            visit.CancelReason = dto.CancelReason;
        }

        await visitRepository.UpdateAsync(visit, ct);

        var visitWithRelations = await visitRepository.GetByIdAsync(id, ct);

        logger.LogInformation("Visit updated [{VisitId}] [{Status}]", id, visit.Status);

        return Response<VisitDto>.Ok(MapToDto(visitWithRelations!));
    }

    public async Task<Response<VisitDto>> CheckoutAsync(long id, long companyId, CancellationToken ct = default)
    {
        var visit = await visitRepository.GetByIdAsync(id, ct);
        if (visit is null || visit.CompanyId != companyId)
        {
            logger.LogWarning("Visit not found [{VisitId}] [{CompanyId}]", id, companyId);
            return Response<VisitDto>.Fail("Visit not found", ErrorCode.NotFound);
        }

        if (visit.Status == "Completed" || visit.Status == "Cancelled")
        {
            return Response<VisitDto>.Fail("Visit already completed or cancelled", ErrorCode.ValidationError);
        }

        visit.CheckOutTime = DateTime.UtcNow;
        visit.Status = "Completed";

        await visitRepository.UpdateAsync(visit, ct);

        var visitWithRelations = await visitRepository.GetByIdAsync(id, ct);

        logger.LogInformation("Visit checked out [{VisitId}]", id);

        return Response<VisitDto>.Ok(MapToDto(visitWithRelations!));
    }

    public async Task<Response<VisitDto>> CancelAsync(long id, string reason, long companyId,
        CancellationToken ct = default)
    {
        var visit = await visitRepository.GetByIdAsync(id, ct);
        if (visit is null || visit.CompanyId != companyId)
        {
            logger.LogWarning("Visit not found [{VisitId}] [{CompanyId}]", id, companyId);
            return Response<VisitDto>.Fail("Visit not found", ErrorCode.NotFound);
        }

        if (visit.Status == "Completed" || visit.Status == "Cancelled")
        {
            return Response<VisitDto>.Fail("Visit already completed or cancelled", ErrorCode.ValidationError);
        }

        visit.Status = "Cancelled";
        visit.CancelReason = reason;

        await visitRepository.UpdateAsync(visit, ct);

        var visitWithRelations = await visitRepository.GetByIdAsync(id, ct);

        logger.LogInformation("Visit cancelled [{VisitId}] [{Reason}]", id, reason);

        return Response<VisitDto>.Ok(MapToDto(visitWithRelations!));
    }

    private static VisitDto MapToDto(Visit visit)
    {
        return new VisitDto(
            visit.Id,
            visit.CompanyId,
            visit.UserId,
            visit.User.FullName,
            visit.CustomerId,
            visit.Customer.Name,
            visit.CheckInTime,
            visit.CheckOutTime,
            visit.CheckInLat,
            visit.CheckInLng,
            visit.Status,
            visit.CancelReason,
            visit.CreatedAt,
            visit.UpdatedAt
        );
    }
}
