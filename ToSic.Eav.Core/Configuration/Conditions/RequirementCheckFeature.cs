using ToSic.Lib.DI;

namespace ToSic.Eav.Configuration
{
    public class RequirementCheckFeature: RequirementCheckBase
    {
        public const string ConditionIsFeature = "feature";

        public RequirementCheckFeature(LazyInit<IFeaturesInternal> features) => Features = features;
        private LazyInit<IFeaturesInternal> Features { get; }

        public override string NameId => ConditionIsFeature;

        public override bool IsOk(Condition condition) => Features.Value.IsEnabled(condition.NameId);

        public override string InfoIfNotOk(Condition condition) 
            => $"The feature '{condition.NameId}' is not enabled - see https://r.2sxc.org/features.";
    }
}
