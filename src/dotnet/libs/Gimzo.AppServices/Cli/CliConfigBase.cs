namespace Gimzo.AppServices.Cli;

public abstract class CliConfigBase(string appName, string appVersion, string? description = null)
{
    public string AppName { get; } = appName;
    public string AppVersion { get; } = appVersion;
    public string? Description { get; } = description;
    public bool Verbose { get; set; }
    public bool ShowHelp { get; set; }
}
