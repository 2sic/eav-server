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
            var results = _appCtxPlus.AppState.ContentTypes.GetAllScopesWithLabels();
            return wrapLog.Return(results);
        }

        #endregion
        
    }
}
