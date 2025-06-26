using System.Xml.Linq;
using ToSic.Eav.Data.Sys.Dimensions;

namespace ToSic.Eav.ImportExport.Sys.Xml;
internal class XmlValueListHelper(string envDefLang, ICollection<TargetLanguageToSourceLanguage> langMap, ILog parentLog) : HelperBase(parentLog, "Xml.ValHlp")
{
    private readonly IList<TargetLanguageToSourceLanguage> _envLangsSortedByPriority = langMap
        .OrderByDescending(p => p.Matches(envDefLang))
        .ThenBy(p => p.EnvironmentKey)
        .ToListOpt();

    private readonly List<XmlValueToImport> _tempTargetValues = [];

    internal List<XmlValueToImport> CreateList(IList<XElement> xmlValuesOfAttrib)
    {
        // protect from accidental multiple calls (needs to be recreated every time)
        if (_tempTargetValues.Count > 0)
            throw new InvalidOperationException("XmlValueListHelper can only be used once. Please create a new instance for each use.");

        // Process each target's language
        foreach (var envLang in _envLangsSortedByPriority)
        {
            var maybeExactMatch = FindAttribWithLanguageMatch(envLang, xmlValuesOfAttrib);
            var sourceValueNode = maybeExactMatch.Element;
            var readOnly = maybeExactMatch.ReadOnly;

            // Take first value if there is only one value without a dimension (default / fallback value), but only in primary language
            if (sourceValueNode == null && xmlValuesOfAttrib.Count > 0 && envLang.Matches(envDefLang))
                sourceValueNode = GetFallbackAttributeInXml(xmlValuesOfAttrib);

            // Override ReadOnly for primary target language
            if (envLang.Matches(envDefLang))
                readOnly = false;

            // Process found value
            if (sourceValueNode != null)
                AddNodeToImportListOrEnhancePrevious(sourceValueNode, envLang, readOnly);
        }

        return _tempTargetValues;
    }

    /// <summary>
    /// Either add the node to the import list with the dimensions, 
    /// OR if it is already in the queue, add the dimension information
    /// </summary>
    /// <param name="sourceValueNode"></param>
    /// <param name="envLang"></param>
    /// <param name="readOnly"></param>
    private void AddNodeToImportListOrEnhancePrevious(XElement sourceValueNode, TargetLanguageToSourceLanguage envLang, bool readOnly)
    {
        var l = Log.Fn();
        var logText = "";
        var dimensionsToAdd = new List<ILanguage>();
        if (langMap.Single(p => p.Matches(envLang.EnvironmentKey)).DimensionId > 0)
        {
            dimensionsToAdd.Add(new Language(envLang.EnvironmentKey, readOnly));
            logText += "built dimension-list";
        }

        // If value has already been added to the list, add just dimension with original ReadOnly state
        var existingImportValue = _tempTargetValues.FirstOrDefault(p => p.XmlValue == sourceValueNode);
        if (existingImportValue != null)
        {
            existingImportValue.Dimensions.AddRange(dimensionsToAdd);
            logText += "targetNode already used for another node, just added dimension";
        }
        else
        {
            _tempTargetValues.Add(new()
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
        var l = Log.Fn<(XElement? Element, bool ReadOnly)>(envLang.EnvironmentKey);
        XElement? sourceValueNode = null;
        var readOnly = false;

        // find the xml-node which best matches the language we want to fill in
        foreach (var sourceLanguage in envLang.PrioritizedDimensions)
        {
            var dimensionId = sourceLanguage.DimensionId.ToString();
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

            l.A($"node for {envLang.EnvironmentKey} on Dim:{sourceLanguage.DimensionId}; readOnly: {readOnly}");
            break;
        }

        return l.Return((sourceValueNode, readOnly), (sourceValueNode != null).ToString());
    }

    private XElement GetFallbackAttributeInXml(ICollection<XElement> xmlValuesOfAttrib)
    {
        var l = Log.Fn<XElement>();
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
        if (sourceValueNode == null)
        {
            l.W("node still null - this indicates a problem! will just use first match");
            sourceValueNode = xmlValuesOfAttrib.First();
        }
        return l.Return(sourceValueNode);
    }

}
