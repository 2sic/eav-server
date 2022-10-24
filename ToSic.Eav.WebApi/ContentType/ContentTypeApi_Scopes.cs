using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;

namespace ToSic.Eav.WebApi
{
    public partial class ContentTypeApi
    {
        #region Scopes

        public IDictionary<string, string> Scopes()
        {
            var wrapLog = Log.Fn<IDictionary<string, string>>();
            var appState = _appStates.Get(_appId);

            var results = appState.ContentTypes.GetAllScopesWithLabels();
            return wrapLog.Return(results);
        }

        #endregion
        
    }
}
