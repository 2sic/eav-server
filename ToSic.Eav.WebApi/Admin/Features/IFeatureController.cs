using System.Collections.Generic;
using ToSic.Eav.Configuration;

namespace ToSic.Eav.WebApi.Admin.Features
{
    public interface IFeatureController
    {
        bool SaveNew(List<FeatureManagementChange> changes);
    }
}