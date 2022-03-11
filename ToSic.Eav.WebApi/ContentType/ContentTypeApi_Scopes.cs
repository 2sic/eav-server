using System.Collections.Generic;
using ToSic.Eav.Data;

namespace ToSic.Eav.WebApi
{
    public partial class ContentTypeApi
    {
        #region Scopes

        public IDictionary<string, string> Scopes()
        {
            var wrapLog = Log.Call<IDictionary<string, string>>();
            var appState = _appStates.Get(_appId);

            var results = appState.ContentTypes.GetAllScopesWithLabels();
            return wrapLog(null, results);
        }

        #endregion
        
    }
}
