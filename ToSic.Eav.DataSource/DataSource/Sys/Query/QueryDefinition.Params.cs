﻿using System.Text.RegularExpressions;
using ToSic.Eav.LookUp;
using ToSic.Eav.LookUp.Sources;

namespace ToSic.Eav.DataSource.Internal.Query;

partial class QueryDefinition
{
    /// <summary>
    /// The raw Params used in this query, as stored in the IEntity
    /// </summary>
    private string Params => GetThis<string>("");

    /// <summary>
    /// The param-dictionary used for the LookUp. All keys will be available in the token [Params:key]
    /// </summary>
    [field: AllowNull, MaybeNull]
    public IDictionary<string, string> ParamsDic
    {
        get => field ??= GenerateParamsDic(Params, Log);
        set;
    }

    /// <summary>
    /// The <see cref="ILookUp"/> for the params of this query - based on the Params.
    /// </summary>
    /// <returns>Always returns a valid ILookup, even if no params found. </returns>
    [field: AllowNull, MaybeNull]
    public ILookUp ParamsLookUp
    {
        get => field ??= new LookUpInDictionary(DataSourceConstants.ParamsSourceName, ParamsDic);
        set;
    }

    /// <summary>
    /// Regex to detect key=value. <br/>
    /// Keys must always be the first thing optionally followed by a = and then anything till a newline.
    /// Anything that doesn't match will be ignored. <br/>
    /// Comments should start with a //
    /// </summary>
    public static Regex ParamRegex = new(
        $@"^(?<{KeyProperty}>\w+)(\=(?<{KeyValue}>[^\r\n]*)?)?",
        RegexOptions.Compiled | RegexOptions.Multiline);

    /// <summary>
    /// Generate the LookUp
    /// They are in the format k=value or key=[some:token]
    /// </summary>
    [PrivateApi]
    public static IDictionary<string, string> GenerateParamsDic(string? paramsText, ILog log)
    {
        var l = log.Fn<IDictionary<string,string>>();

        var paramsDic = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        if (string.IsNullOrWhiteSpace(paramsText))
            return l.Return(paramsDic, "no params");

        // extract the lines which look like key=value
        var paramMatches = ParamRegex.Matches(paramsText);
        log.A($"found {paramMatches.Count} params");

        foreach (Match testParam in paramMatches)
        {
            var key = testParam.Groups[KeyProperty].Value.ToLowerInvariant();
            var value = testParam.Groups[KeyValue].Value;
            log.A($"Params:{key}={value}");
            if (!paramsDic.ContainsKey(key))
                paramsDic[key] = value; // add not-yet-added-value
            else
                log.A($"Params:{key} already existed, will leave as is");
        }

        return l.Return(paramsDic, paramsDic.Count.ToString());
    }

    /// <summary>
    /// Will reset all the parameters so you can run the query again with different parameters. 
    /// </summary>
    public void Reset()
    {
        Log.A($"{nameof(Reset)}()");
        ParamsDic = null!;
        ParamsLookUp = null!;
    }
}