using System.Collections.Generic;
using ToSic.Eav.Configuration;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface ISystemController
    {
        IEnumerable<Feature> Features(int appId);
    }
}