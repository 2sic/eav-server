using ToSic.Eav.SysData;

namespace ToSic.Eav.WebApi.Context
{
    public class FeatureDto
    {
        public FeatureDto(FeatureState state)
        {
            NameId = state.NameId;
            IsEnabled = state.IsEnabled;
            Name = state.Definition.Name;
        }

        public string NameId { get; }
        public bool IsEnabled { get; }
        public string Name { get; }
    }
}
