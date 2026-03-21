namespace Nexus.Application.Interfaces.UseCases;

public sealed class SeedDataSettings
{
    public const string SectionName = "SeedData";
    
    public bool RunOnStartup { get; set; } = true;
    public string AdminUsername { get; set; } = "admin";
    public string AdminPassword { get; set; } = string.Empty;
}
