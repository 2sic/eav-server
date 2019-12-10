using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSources.Queries
{
    public partial class QueryDefinition
    {
        /// <summary>
        /// The raw Params used in this query, as stored in the IEntity
        /// </summary>
        public string ParamsRaw => Get<string>(FieldParams, null);

		/// <summary>
        /// The param-dictionary used for the LookUp. All keys will be available in the token [Params:key]
        /// </summary>
        public IDictionary<string, string> Params => _params ?? (_params = GenerateParamsDic(ParamsRaw));
        private IDictionary<string, string> _params;

		/// <summary>
        /// The <see cref="ILookUp"/> for the params of this query - based on the Params.
        /// </summary>
        /// <returns>Always returns a valid ILookup, even if no params found. </returns>
        public ILookUp ParamsLookUp => _paraLookUp ?? (_paraLookUp = new LookUpInDictionary(QueryConstants.ParamsLookup, Params));
        private ILookUp _paraLookUp;

        internal static Regex ParamRegex = new Regex(
            $@"(?<{KeyProperty}>\w+)=(?<{KeyValue}>[^\r\n]*)", RegexOptions.Compiled);

        /// <summary>
        /// Generate the LookUp
        /// They are in the format k=value or key=[some:token]
        /// </summary>
        internal IDictionary<string, string> GenerateParamsDic(string paramsText)
        {
            var wrapLog = Log.Call<IDictionary<string,string>>(nameof(GenerateParamsDic), $"{Entity.EntityId}");

            var paramsDic = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            if (string.IsNullOrWhiteSpace(paramsText)) return wrapLog("no params", paramsDic);

            // extract the lines which look like key=value
            var paramMatches = ParamRegex.Matches(paramsText);
            Log.Add($"found {paramMatches.Count} params");

            foreach (Match testParam in paramMatches)
            {
                var key = testParam.Groups[KeyProperty].Value.ToLower();
                var value = testParam.Groups[KeyValue].Value;
                Log.Add($"Params:{key}={value}");
                if (!paramsDic.ContainsKey(key))
                    paramsDic[key] = value; // add not-yet-added-value
                else
                    Log.Add($"Params:{key} already existed, will leave as is");
            }

            return wrapLog(paramsDic.Count.ToString(), paramsDic);
        }

        /// <summary>
        /// Will reset all the parameters so you can run the query again with different parameters. 
        /// </summary>
        public void Reset()
        {
            Log.Add($"{nameof(Reset)}()");
            _params = null;
            _paraLookUp = null;
        }
    }
}
