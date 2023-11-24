using System.Collections.Generic;
using ToSic.Eav.Internal.Features;

namespace ToSic.Eav.WebApi.Admin.Features;

public interface IFeatureController
{
    bool SaveNew(List<FeatureManagementChange> changes);
}