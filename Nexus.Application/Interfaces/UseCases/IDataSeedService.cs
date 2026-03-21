namespace Nexus.Application.Interfaces.UseCases;

public interface IDataSeedService
{
    public Task SeedAsync(CancellationToken ct = default);
}
