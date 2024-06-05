using ToSic.Eav.Apps.Internal.Insights;
using ToSic.Eav.WebApi.Errors;
using ToSic.Lib.Coding;

namespace ToSic.Eav.WebApi.Sys.Insights;

internal abstract class InsightsProviderWeb(string name, NoParamOrder protect = default, string teaser = default, string helpCategory = default, object[] connect = default) :InsightsProvider(name, protect, teaser, helpCategory, connect)
{
    protected static Exception CreateBadRequest(string msg) => HttpException.BadRequest(msg);

}