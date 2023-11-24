using System.Text.Json.Serialization;

namespace ToSic.Eav.SysData;

/// <summary>
/// Experimental - base class for any kind of aspect and it's state
/// </summary>
public class AspectState<TAspect> where TAspect : Aspect
{
    public AspectState(TAspect aspect, bool isEnabled)
    {
        Aspect = aspect;
        IsEnabled = isEnabled;
    }

    /// <summary>
    /// Feature Definition.
    /// </summary>
    [JsonIgnore]
    public TAspect Aspect { get; }

    public virtual bool IsEnabled { get; }
}