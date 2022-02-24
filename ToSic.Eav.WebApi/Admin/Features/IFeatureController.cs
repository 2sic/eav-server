using System.Collections.Generic;
using ToSic.Eav.Configuration;

namespace ToSic.Eav.WebApi.Admin.Features
{
    public interface IFeatureController
    {
        // TODO: PROBABLY REMOVE, PROBABLY NOT USED ANY MORE
        //IEnumerable<FeatureState> List(bool reload = false);

        // v13.02 not used any more
        //string RemoteManageUrl();

        // TODO: PROBABLY REMOVE, PROBABLY NOT USED ANY MORE
        //bool Save(FeaturesDto featuresManagementResponse);

        bool SaveNew(List<FeatureNewDto> featuresManagementResponse);
    }
}