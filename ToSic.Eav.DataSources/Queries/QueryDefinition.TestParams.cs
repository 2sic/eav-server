using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ToSic.Eav.Documentation;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSources.Queries
{
    public partial class QueryDefinition
    {
        internal const string KeyToken = "Token",
            KeyProperty = "Property",
            KeyValue = "Value";

        internal static Regex TestParamRegex = new Regex(
            $@"(?:\[(?<{KeyToken}>\w+):(?<{KeyProperty}>\w+)\])=(?<{KeyValue}>[^\r\n]*)", RegexOptions.Compiled);



        /// <summary>
        /// The test parameters as stored in the IEntity
        /// </summary>
        public string TestParameters => Get<string>(FieldTestParams, null);

        [PrivateApi]
        public IEnumerable<ILookUp> TestParameterLookUps => GenerateTestValueLookUps();

        /// <summary>
        /// Retrieve test values to test a specific query. 
        /// The specs are found in the query definition, but the must be converted to a source
        /// They are in the format [source:key]=value
        /// </summary>
        /// <returns></returns>
        private IEnumerable<ILookUp> GenerateTestValueLookUps()
        {
            var wrapLog = Log.Call($"{Entity.EntityId}");
            // Parse Test-Parameters in Format [Token:Property]=Value
            var testParameters = TestParameters;//  ((IAttribute<string>) qdef.Entity[QueryDefinition.FieldTestParams]).TypedContents;
            if (testParameters == null)
                return null;
            // extract the lines which look like [source:property]=value
            var testValueTokens = TestParamRegex.Matches(testParameters);// Regex.Matches(testParameters, $@"(?:\[(?<{KeyToken}>\w+):(?<{KeyProperty}>\w+)\])=(?<{KeyValue}>[^\r\n]*)");

            // Create a list of static Property Accessors
            var result = new List<ILookUp>();
            foreach (Match testParam in testValueTokens)
            {
                var token = testParam.Groups[KeyToken].Value.ToLowerInvariant();

                // Ensure a PropertyAccess exists for this LookUp name
                if (!(result.FirstOrDefault(i => i.Name == token) is LookUpInDictionary currentLookUp))
                {
                    currentLookUp = new LookUpInDictionary(token);
                    result.Add(currentLookUp);
                }

                // Add the static value
                currentLookUp.Properties.Add(testParam.Groups[KeyProperty].Value, testParam.Groups[KeyValue].Value);
            }
            wrapLog("ok");
            return result;
        }

    }
}
