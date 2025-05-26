using System.Text.Json.Serialization;

namespace ToSic.Eav.SysData;

/// <summary>
/// Experimental - base class for any kind of aspect and it's state
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public record AspectState<TAspect>(TAspect Aspect, bool IsEnabled)
    where TAspect : Aspect
{
    /// <summary>
    /// Feature Definition.
    /// </summary>
    [JsonIgnore]
    public TAspect Aspect { get; } = Aspect;

    public virtual bool IsEnabled { get; } = IsEnabled;
}