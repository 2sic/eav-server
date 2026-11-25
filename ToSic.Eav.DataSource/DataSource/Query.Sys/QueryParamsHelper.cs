namespace ToSic.Eav.DataSource.Query.Sys;

/// <summary>
/// Shared helper for adding parameters to a query before execution.
/// </summary>
/// <param name="definition"></param>
internal class QueryParamsHelper(QueryDefinition definition, Func<bool> getAlreadyCompiled, ILog log)
{
    public void Params(string key, string? value)
    {
        var l = log.Fn($"{key}, {value}");
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));
        // if the query has already been built, and we're changing a value, make sure we'll regenerate the results
        if (getAlreadyCompiled())
        {
            l.A("Can't set param - query already compiled, error");
            throw new("Can't set param any more, the query has already been compiled. " +
                      "Always set params before accessing the data. " +
                      "To Re-Run the query with other params, call Reset() first.");
        }

        definition.ParamsDic[key] = value ?? "";
        l.Done();
    }


    public void Params(string list)
        => Params(QueryDefinitionParams.GenerateParamsDic(list, log));


    public void Params(IDictionary<string, string> values)
    {
        foreach (var qP in values)
            Params(qP.Key, qP.Value);
    }

}