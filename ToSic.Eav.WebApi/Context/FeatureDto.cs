using ToSic.Eav.SysData;

namespace ToSic.Eav.WebApi.Context;

public class FeatureDto(FeatureState state)
{
    public string NameId { get; } = state.NameId;
    public bool IsEnabled { get; } = state.IsEnabled;
    public string Name { get; } = state.Aspect.Name;
}