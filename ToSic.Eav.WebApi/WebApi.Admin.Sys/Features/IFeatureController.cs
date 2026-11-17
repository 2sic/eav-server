using ToSic.Eav.WebApi.Sys.Licenses;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.WebApi.Admin.Sys;

public interface IFeatureController: IAppAdminWebApiController
{
    // UI test: Open any feature-detail dialog from Apps management > Features / License.
    FeatureStateDto Details(string nameId);

    // UI test: Apps management > Features / License > toggle or configure a feature and save.
    bool SaveNew(List<FeatureStateChange> changes);
}
