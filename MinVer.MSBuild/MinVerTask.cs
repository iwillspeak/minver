#if NET
using System.Globalization;
#endif
using System.Reflection;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MinVer.MSBuild;

public class MinVerTask : ToolTask
{
    public string? AutoIncrement { get; set; }

    public string? BuildMetadata { get; set; }

    public string? DefaultPreReleaseIdentifiers { get; set; }

    public string? DefaultPreReleasePhase { get; set; }

    public string? IgnoreHeight { get; set; }

    public string? MinimumMajorMinor { get; set; }

    public string? ProjectDirectory { get; set; }

    public string? TagPrefix { get; set; }

    public string? TargetFramework { get; set; }

    public string? Verbosity { get; set; }

    public string? VersionOverride { get; set; }

    [Output]
    public string? Version { get; set; }

    protected override MessageImportance StandardErrorLoggingImportance => this.Verbosity is "detailed" or "d" or "diagnostic" or "diag" ? MessageImportance.High : MessageImportance.Low;

    protected override string ToolName => "dotnet";

    protected override string GenerateCommandLineCommands()
    {
        var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var minVerPath = Path.GetFullPath(Path.Combine(assemblyDirectory!, "..", this.TargetFramework!, "MinVer.dll"));

        var builder = new StringBuilder();

#if NET
        _ = builder.Append(CultureInfo.InvariantCulture, $"\"{minVerPath}\" ");
        _ = builder.Append(CultureInfo.InvariantCulture, $"--auto-increment \"{this.AutoIncrement}\" ");
        _ = builder.Append(CultureInfo.InvariantCulture, $"--build-metadata \"{this.BuildMetadata}\" ");
        _ = builder.Append(CultureInfo.InvariantCulture, $"--default-pre-release-identifiers \"{this.DefaultPreReleaseIdentifiers}\" ");
        _ = builder.Append(CultureInfo.InvariantCulture, $"--default-pre-release-phase \"{this.DefaultPreReleasePhase}\" ");

        if (this.IgnoreHeight?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false)
        {
            _ = builder.Append("--ignore-height ");
        }

        _ = builder.Append(CultureInfo.InvariantCulture, $"--minimum-major-minor \"{this.MinimumMajorMinor}\" ");
        _ = builder.Append(CultureInfo.InvariantCulture, $"--tag-prefix \"{this.TagPrefix}\" ");
        _ = builder.Append(CultureInfo.InvariantCulture, $"--verbosity \"{this.Verbosity}\" ");
        _ = builder.Append(CultureInfo.InvariantCulture, $"--version-override \"{this.VersionOverride}\" ");
#else
        _ = builder.Append($"\"{minVerPath}\" ");
        _ = builder.Append($"--auto-increment \"{this.AutoIncrement}\" ");
        _ = builder.Append($"--build-metadata \"{this.BuildMetadata}\" ");
        _ = builder.Append($"--default-pre-release-identifiers \"{this.DefaultPreReleaseIdentifiers}\" ");
        _ = builder.Append($"--default-pre-release-phase \"{this.DefaultPreReleasePhase}\" ");

        if (this.IgnoreHeight?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false)
        {
            _ = builder.Append("--ignore-height ");
        }

        _ = builder.Append($"--minimum-major-minor \"{this.MinimumMajorMinor}\" ");
        _ = builder.Append($"--tag-prefix \"{this.TagPrefix}\" ");
        _ = builder.Append($"--verbosity \"{this.Verbosity}\" ");
        _ = builder.Append($"--version-override \"{this.VersionOverride}\" ");
#endif

        return builder.ToString();
    }

    protected override string GenerateFullPathToTool() => "dotnet";

    protected override string? GetWorkingDirectory() => this.ProjectDirectory;

    protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
    {
        if (singleLine != null && !singleLine.StartsWith("MinVer", StringComparison.Ordinal))
        {
            this.Version = singleLine;
            this.CacheVersion(singleLine);
        }

        base.LogEventsFromTextOutput(singleLine, messageImportance);
    }

    protected override bool SkipTaskExecution()
    {
        if (this.GetCachedVersionOrDefault() is var version && version == null)
        {
            return false;
        }

        this.Log.LogMessage(this.StandardErrorLoggingImportance, "MinVer: Skipping task execution and using cached version {0}", version);
        this.Version = version;
        return true;
    }

    private object CacheKey => (this.AutoIncrement, this.BuildMetadata, this.DefaultPreReleaseIdentifiers, this.DefaultPreReleasePhase, this.IgnoreHeight, this.MinimumMajorMinor, this.TagPrefix, this.Verbosity, this.VersionOverride);

    private void CacheVersion(string version) => this.BuildEngine4.RegisterTaskObject(this.CacheKey, version, RegisteredTaskObjectLifetime.Build, allowEarlyCollection: true);

    private string? GetCachedVersionOrDefault() => (string)this.BuildEngine4.GetRegisteredTaskObject(this.CacheKey, RegisteredTaskObjectLifetime.Build);
}
