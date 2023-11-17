using ToSic.Eav.Internal.Requirements;
using ToSic.Eav.SysData;
using ToSic.Lib.DI;

namespace ToSic.Eav.Internal.Features
{
    public class FeatureRequirementCheck: RequirementCheckBase
    {
        public const string ConditionIsFeature = "feature";

        public FeatureRequirementCheck(LazySvc<IEavFeaturesService> features) => Features = features;
        private LazySvc<IEavFeaturesService> Features { get; }

        public override string NameId => ConditionIsFeature;

        public override bool IsOk(Requirement requirement) => Features.Value.IsEnabled(requirement.NameId);

        public override string InfoIfNotOk(Requirement requirement) 
            => $"The feature '{requirement.NameId}' is not enabled - see https://go.2sxc.org/features.";
    }
}
