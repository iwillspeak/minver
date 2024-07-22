using Microsoft.Build.Framework;
using Task = Microsoft.Build.Utilities.Task;

namespace MSBuild.Caching;

public class CacheSet : Task
{
    public string? Key { get; set; }

    public string? Value { get; set; }

    public override bool Execute()
    {
        this.BuildEngine4.RegisterTaskObject(this.Key, this.Value, RegisteredTaskObjectLifetime.Build, default);
        return true;
    }
}
