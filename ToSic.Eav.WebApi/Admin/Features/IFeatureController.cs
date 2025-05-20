using ToSic.Eav.Internal.Features;
using ToSic.Eav.WebApi.Sys.Licenses;

namespace ToSic.Eav.WebApi.Admin.Features;

public interface IFeatureController
{
    FeatureStateDto Details(string nameId);
    bool SaveNew(List<FeatureStateChange> changes);
}