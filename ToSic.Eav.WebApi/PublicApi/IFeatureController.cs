using System.Collections.Generic;
using ToSic.Eav.Configuration;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IFeatureController
    {
        IEnumerable<FeatureState> List(bool reload = false);

        string RemoteManageUrl();
    }
}