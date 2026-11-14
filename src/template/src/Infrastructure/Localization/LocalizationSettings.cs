namespace Genocs.Microservice.Template.Infrastructure.Localization;

public class LocalizationSettings
{
    public bool Enabled { get; set; }
    public string? ResourcesPath { get; set; }
    public string[]? SupportedCultures { get; set; }
    public string? DefaultRequestCulture { get; set; }
    public bool FallbackToParent { get; set; } = true;
}