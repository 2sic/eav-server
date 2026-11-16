using ToSic.Eav.WebApi.Admin.Sys;
using ToSic.Eav.WebApi.Sys.Licenses;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.WebApi.Sys.Admin.Features;

public interface IFeatureController: IAppAdminWebApiController
{
    FeatureStateDto Details(string nameId);
    bool SaveNew(List<FeatureStateChange> changes);
}