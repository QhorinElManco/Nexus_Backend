using Nexus.Application.Dto.Sales;
using Nexus.Application.Dto.Response;

namespace Nexus.Application.Interfaces.UseCases;

public interface IVisitService
{
    public Task<Response<VisitDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default);
    public Task<Response<IReadOnlyList<VisitDto>>> GetByCompanyAsync(long companyId, CancellationToken ct = default);

    public Task<ResponsePagination<VisitDto>> SearchAsync(VisitSearchRequest request, long companyId,
        CancellationToken ct = default);

    public Task<Response<VisitDto>> CreateAsync(CreateVisitDto dto, long companyId, long userId,
        CancellationToken ct = default);

    public Task<Response<VisitDto>> UpdateAsync(long id, UpdateVisitDto dto, long companyId,
        CancellationToken ct = default);

    public Task<Response<VisitDto>> CheckoutAsync(long id, long companyId, CancellationToken ct = default);
    public Task<Response<VisitDto>> CancelAsync(long id, string reason, long companyId, CancellationToken ct = default);
}
