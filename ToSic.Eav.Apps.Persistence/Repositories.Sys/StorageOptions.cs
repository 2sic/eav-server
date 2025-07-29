using ToSic.Eav.Apps;
using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Data.Sys.Save;

namespace ToSic.Eav.Repositories.Sys;
public record StorageOptions(int? ZoneId, int? AppId, int? ParentAppId = null, LogSettings? LogSettings = default, SaveProcessOptions? ProcessOptions = default)
{
    public StorageOptions(IAppReader appReader) : this(appReader.ZoneId, appReader.AppId, appReader.GetParentCache()?.AppId)
    { }

    public LogSettings LogSettings { get; init; } = LogSettings ?? new();

    public SaveProcessOptions ProcessOptions { get; init; } = ProcessOptions ?? new();
}
