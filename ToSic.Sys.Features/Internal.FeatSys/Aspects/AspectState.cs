using System.Text.Json.Serialization;

namespace ToSic.Eav.SysData;

/// <summary>
/// Experimental - base class for any kind of aspect and it's state
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AspectState<TAspect>(TAspect aspect, bool isEnabled)
    where TAspect : Aspect
{
    /// <summary>
    /// Feature Definition.
    /// </summary>
    [JsonIgnore]
    public TAspect Aspect { get; } = aspect;

    public virtual bool IsEnabled { get; } = isEnabled;
}