namespace Nexus.Application.Interfaces.UseCases;

public interface IClaimsExtractor
{
    public long GetCurrentCompanyId();
    public long GetCurrentUserId();
    public IReadOnlyList<string> GetCurrentRoles();
    public IReadOnlyList<string> GetCurrentPermissions();
    public bool TryGetCurrentCompanyId(out long companyId);
}
