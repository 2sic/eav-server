using System.Xml.Linq;
using ToSic.Eav.Data.Sys.Dimensions;

namespace ToSic.Eav.ImportExport.Sys.Xml;
internal class XmlValueListHelper(XmlToEntity.Options options, ILog parentLog) : HelperBase(parentLog, "Xml.ValHlp")
{
    #region Detailed Logging

    /// <summary>
    /// Logger for the details of the deserialization process.
    /// Goal is that it can be enabled/disabled as needed.
    /// </summary>
    private ILog? LogDetails => field ??= Log.IfDetails(options.LogSettings);

    #endregion

    private readonly IList<TargetLanguageToSourceLanguage> _envLangsSortedByPriority = options.EnvLangs
        .OrderByDescending(p => p.Matches(options.EnvDefLang))
        .ThenBy(p => p.EnvironmentKey)
        .ToListOpt();

    internal List<XmlValueToImport> CreateList(IList<XElement> xmlValuesOfAttrib)
    {
        // protect from accidental multiple calls (needs to be recreated every time)
        var targetVals = new List<XmlValueToImport>();

        // Process each target's language
        foreach (var envLang in _envLangsSortedByPriority)
        {
            var maybeExactMatch = FindAttribWithLanguageMatch(envLang, xmlValuesOfAttrib);
            var sourceValueNode = maybeExactMatch.Element;
            var readOnly = maybeExactMatch.ReadOnly;

            // Take first value if there is only one value without a dimension (default / fallback value), but only in primary language
            if (sourceValueNode == null && xmlValuesOfAttrib.Count > 0 && envLang.Matches(options.EnvDefLang))
                sourceValueNode = GetFallbackAttributeInXml(xmlValuesOfAttrib);

            // Override ReadOnly for primary target language
            if (envLang.Matches(options.EnvDefLang))
                readOnly = false;

            // Process found value
            if (sourceValueNode != null)
                AddNodeToImportListOrEnhancePrevious(targetVals, sourceValueNode, envLang, readOnly);
        }

        return targetVals;
    }

    /// <summary>
    /// Either add the node to the import list with the dimensions, 
    /// OR if it is already in the queue, add the dimension information
    /// </summary>
    private void AddNodeToImportListOrEnhancePrevious(List<XmlValueToImport> targetValues, XElement sourceValueNode, TargetLanguageToSourceLanguage envLang, bool readOnly)
    {
        var l = LogDetails.Fn();
        var logText = "";
        var dimensionsToAdd = new List<ILanguage>();
        if (options.EnvLangs.Single(p => p.Matches(envLang.EnvironmentKey)).Id > 0)
        {
            dimensionsToAdd.Add(new Language(envLang.EnvironmentKey, readOnly));
            logText += "built dimension-list";
        }

        // If value has already been added to the list, add just dimension with original ReadOnly state
        var existingImportValue = targetValues.FirstOrDefault(p => p.XmlValue == sourceValueNode);
        if (existingImportValue != null)
        {
            existingImportValue.Dimensions.AddRange(dimensionsToAdd);
            logText += "targetNode already used for another node, just added dimension";
        }
        else
        {
            targetValues.Add(new()
            {
                Dimensions = dimensionsToAdd,
                XmlValue = sourceValueNode
            });
            logText += "targetNode was not used yet, added it";
        }

        l.Done(logText);
    }

    private (XElement? Element, bool ReadOnly) FindAttribWithLanguageMatch(TargetLanguageToSourceLanguage envLang, ICollection<XElement> xmlValuesOfAttrib)
    {
        var l = LogDetails.Fn<(XElement? Element, bool ReadOnly)>(envLang.EnvironmentKey);
        XElement? sourceValueNode = null;
        var readOnly = false;

        // find the xml-node which best matches the language we want to fill in
        foreach (var sourceLanguage in envLang.PrioritizedDimensions)
        {
            var dimensionId = sourceLanguage.Id.ToString();
            // find a possible match for exactly this language
            sourceValueNode = xmlValuesOfAttrib
                .FirstOrDefault(p =>
                    p.Elements(XmlConstants.ValueDimNode)
                        .Any(d => d.Attribute(XmlConstants.DimId)?.Value == dimensionId)
                );
            if (sourceValueNode == null)
                continue;

            // if match found, check what the read/write should be
            var textVal = sourceValueNode
                .Elements(XmlConstants.ValueDimNode)
                .FirstOrDefault(p => p.Attribute(XmlConstants.DimId)?.Value == dimensionId)?
                .Attribute("ReadOnly")?.Value ?? "false";

            readOnly = bool.Parse(textVal);

            l.A($"node for {envLang.EnvironmentKey} on Dim:{sourceLanguage.Id}; readOnly: {readOnly}");
            break;
        }

        return l.Return((sourceValueNode, readOnly), (sourceValueNode != null).ToString());
    }

    private XElement GetFallbackAttributeInXml(ICollection<XElement> xmlValuesOfAttrib)
    {
        var l = LogDetails.Fn<XElement>();
        // First, try to take a fallback node without language assignments
        var sourceValueNode = xmlValuesOfAttrib
            .FirstOrDefault(xv =>
            {
                var dimNodes = xv.Elements(XmlConstants.ValueDimNode).ToListOpt();
                // keep it if it has no dimensions, or if it has a dimensionId of 0
                return !dimNodes.Any() || dimNodes.Any(x => x.Attribute(XmlConstants.DimId)?.Value == "0");
            });


        // Still nothing found, just take the first one, no matter what's language it's for
        // this should probably never happen, but just in case...
        return sourceValueNode == null
            ? l.Return(xmlValuesOfAttrib.First(), "node still null - this indicates a problem! will just use first match")
            : l.Return(sourceValueNode);
    }

}
