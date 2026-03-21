namespace Nexus.Application.Interfaces.UseCases;

public interface IDataSeedService
{
    Task SeedAsync(CancellationToken ct = default);
}
