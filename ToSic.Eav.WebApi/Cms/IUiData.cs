using System.Collections.Generic;
using ToSic.Eav.Configuration;
using ToSic.Eav.Context;
using ToSic.Eav.Security;

namespace ToSic.Eav.WebApi.Cms
{
    /// <summary>
    /// Helper service to retrieve all the UI features for the edit UI
    /// </summary>
    public interface IUiData
    {
        /// <summary>
        /// if the user has full edit permissions, he may also get the un-public features
        /// otherwise just the public Ui features
        /// </summary>
        IList<FeatureState> Features(IContextOfApp appContext, IMultiPermissionCheck permCheck);
    }
}