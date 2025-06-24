﻿using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Dimensions.Sys;
using ToSic.Eav.Data.Entities.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Global;
using ToSic.Eav.Data.Values.Sys;
using ToSic.Eav.Metadata.Targets;

namespace ToSic.Eav.ImportExport.Sys.Xml;

/// <summary>
/// Import EAV Data from XML Format
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class XmlToEntity(IGlobalDataService globalData, DataBuilder dataBuilder)
    : ServiceBase("Imp.XmlEnt", connect: [dataBuilder, globalData])
{

    public XmlToEntity Init(int appId, ICollection<DimensionDefinition> srcLanguages, int? srcDefLang, ICollection<DimensionDefinition> envLanguages, string envDefLang)
    {
        AppId = appId;
        envLanguages = envLanguages
            .OrderByDescending(p => p.Matches(envDefLang))
            .ThenBy(p => p.EnvironmentKey)
            .ToListOpt();
        EnvLangs = PrepareTargetToSourceLanguageMapping(envLanguages, envDefLang, srcLanguages, srcDefLang);
        EnvDefLang = envDefLang;
        // seems unused 2025-06-10 2dm
        //_srcDefLang = srcDefLang?.ToString();
        return this;
    }

    public int AppId { get; private set; }


    private ICollection<TargetLanguageToSourceLanguage> PrepareTargetToSourceLanguageMapping(ICollection<DimensionDefinition> envLanguages, string envDefLang, ICollection<DimensionDefinition> srcLanguages, int? srcDefLang)
    {
        var l = Log.Fn<ICollection<TargetLanguageToSourceLanguage>>($"Env has {envLanguages.Count} languages");
        ICollection<TargetLanguageToSourceLanguage> result;
        // if the environment doesn't have languages defined, we'll create a temp-entry for the main language to allow mapping
        if (envLanguages.Any())
        {
            result = envLanguages
                .Select(el => new TargetLanguageToSourceLanguage
                {
                    Active = el.Active,
                    Key = el.Key,
                    Name = el.Name,
                    EnvironmentKey = el.EnvironmentKey,
                    DimensionId = el.DimensionId,
                    PrioritizedDimensions = FindPriorizedMatchingDimensions(el, envDefLang, srcLanguages, srcDefLang)
                })
                .ToListOpt();
        }
        else
        {
            var tempDimension = new DimensionDefinition
            {
                Active = true,
                DimensionId = 0,
                EnvironmentKey = envDefLang,
                Key = "dummy-not-used",
                Name = "Default"
            };
            var prioritized = FindPriorizedMatchingDimensions(tempDimension, envDefLang, srcLanguages, srcDefLang);
            result =
            [
                new()
                {
                    Active = true,
                    EnvironmentKey = envDefLang,
                    PrioritizedDimensions = prioritized,
                    Name = tempDimension.Name,
                    Key = tempDimension.Key,
                }
            ];
        }

        return l.Return(result, $"LanguageMap has {result.Count} items");
    }

    private List<DimensionDefinition> FindPriorizedMatchingDimensions(DimensionDefinition targetLang, string envDefLang, ICollection<DimensionDefinition> srcLangs, int? srcDefLang)
    {
        var languageMap = new List<DimensionDefinition>();
            
        // Add exact match source language, if exists
        var exactMatchSourceDimension = srcLangs
            .FirstOrDefault(p => targetLang.Matches(p.EnvironmentKey));
        if (exactMatchSourceDimension != null)
            languageMap.Add(exactMatchSourceDimension);

        // Add non-exact match language
        var unExactMatchSourceDimensions = srcLangs
            .Where(sd =>
                !targetLang.Matches(sd.EnvironmentKey) &&
                sd.EnvironmentKey.StartsWith(targetLang.EnvironmentKey.ToLowerInvariant().Substring(0, 2))
            )
            .OrderByDescending(p => p.EnvironmentKey == envDefLang)
            .ThenByDescending(p => p.EnvironmentKey.Length == 2 || p.EnvironmentKey.Length == 5 &&
                p.EnvironmentKey.Substring(0, 2) == p.EnvironmentKey.Substring(3, 2)
            )
            .ThenBy(p => p.EnvironmentKey);
        languageMap.AddRange(unExactMatchSourceDimensions);

        // Add primary language, if current target is primary
        if (targetLang.Matches(envDefLang) && srcDefLang.HasValue)
        {
            var sourcePrimaryLanguage = srcLangs
                .FirstOrDefault(p => p.DimensionId == srcDefLang);
            if (sourcePrimaryLanguage != null && !languageMap.Contains(sourcePrimaryLanguage))
                languageMap.Add(sourcePrimaryLanguage);
        }
            
        return languageMap;
    }

    //private readonly List<string> _relevantSrcLangsByPriority;
    [field: AllowNull, MaybeNull]
    private ICollection<TargetLanguageToSourceLanguage> EnvLangs
    {
        get => field ?? throw new NullReferenceException("XmlToEntity not initialized, call Init() first");
        set;
    }

    [field: AllowNull, MaybeNull]
    private string EnvDefLang
    {
        get => field ?? throw new NullReferenceException("XmlToEntity not initialized, call Init() first");
        set;
    }
    //private string _srcDefLang;
        
    /// <summary>
    /// Returns an EAV import entity
    /// </summary>
    /// <param name="xEntity">xEntity to parse</param>
    /// <param name="mdTarget"></param>
    internal Entity BuildEntityFromXml(XElement xEntity, Target mdTarget)
    {
        var wrap = Log.Fn<Entity>();
        var finalAttributes = new Dictionary<string, IAttribute>();

        // Group values by StaticName
        var valuesGroupedByStaticName = xEntity
            .Elements(XmlConstants.ValueNode)
            .GroupBy(
                v => v.Attribute(XmlConstants.KeyAttr)?.Value,
                e => e, (key, e) => new
                {
                    StaticName = key!,
                    Values = e.ToListOpt()
                }
            );


        //IList<TargetLanguageToSourceLanguage> envLangsSortedByPriority = EnvLangs
        //    .OrderByDescending(p => p.Matches(EnvDefLang))
        //    .ThenBy(p => p.EnvironmentKey)
        //    .ToListOpt();

        // Process each attribute (values grouped by StaticName)
        foreach (var sourceAttrib in valuesGroupedByStaticName)
        {
            var lAttrib = Log.Fn(sourceAttrib.StaticName);
            //var xmlValuesOfAttrib = sourceAttrib.Values;
            //var tempTargetValues = new List<XmlValueToImport>();

            //// Process each target's language
            //foreach (var envLang in envLangsSortedByPriority)
            //{
            //    var maybeExactMatch = FindAttribWithLanguageMatch(envLang, xmlValuesOfAttrib);
            //    var sourceValueNode = maybeExactMatch.Element;
            //    var readOnly = maybeExactMatch.ReadOnly;

            //    // Take first value if there is only one value without a dimension (default / fallback value), but only in primary language
            //    if (sourceValueNode == null && xmlValuesOfAttrib.Count > 0 && envLang.Matches(EnvDefLang))
            //        sourceValueNode = GetFallbackAttributeInXml(xmlValuesOfAttrib);

            //    // Override ReadOnly for primary target language
            //    if (envLang.Matches(EnvDefLang))
            //        readOnly = false;

            //    // Process found value
            //    if (sourceValueNode != null)
            //        AddNodeToImportListOrEnhancePrevious(sourceValueNode, tempTargetValues, envLang, readOnly);
            //}

            // Process each target's language
            var tempTargetValues = new XmlValueListHelper(EnvDefLang, EnvLangs, Log).CreateList(sourceAttrib.Values);

            // construct value elements
            var currentAttributesImportValues = tempTargetValues
                .Select(tempImportValue => dataBuilder.Value.Build(
                        ValueTypeHelpers.Get(
                            tempImportValue.XmlValue.Attribute(XmlConstants.EntityTypeAttribute)?.Value ??
                            throw new NullReferenceException("can't build attribute with unknown value-type")
                        ),
                        tempImportValue.XmlValue.Attribute(XmlConstants.ValueAttr)?.Value ??
                        throw new NullReferenceException("can't build attribute without value"),
                        tempImportValue.Dimensions.ToImmutableSafe()
                    )
                )
                .ToListOpt();

            // construct the attribute with these value elements
            var newAttr = dataBuilder.Attribute.Create(
                sourceAttrib.StaticName,
                ValueTypeHelpers.Get(tempTargetValues.First().XmlValue.Attribute(XmlConstants.EntityTypeAttribute)
                    ?.Value ?? ""),
                currentAttributesImportValues);

            // attach to attributes-list
            finalAttributes.Add(sourceAttrib.StaticName, newAttr);

            lAttrib.Done();
        }

        var typeName = xEntity.Attribute(XmlConstants.AttSetStatic)?.Value;
        if (typeName == null)
            throw new NullReferenceException("trying to import an xml entity but type is null - " + xEntity);
		    
        // find out if it's a system type, and use that if it exists
        var guidString = xEntity.Attribute(XmlConstants.GuidNode)?.Value ??
                         throw new NullReferenceException("can't import an entity without a guid identifier");
        var guid = Guid.Parse(guidString);
        var isPublished = xEntity.Attribute(XmlConstants.IsPublished)?.Value;

        var globalTypeIfFound = globalData.GetContentType(typeName);
        var contentType = globalTypeIfFound;
        if (contentType == null)
        {
            // if it's not a global type but still marked as IsJson
            // then it's a local extension type with Content-Type definitions in the app/system folder
            // in this case, the storage system must know that it should json-save it
            var newTypeRepoType = xEntity.Attribute(XmlConstants.EntityIsJsonAttribute)?.Value == "True"
                ? RepositoryTypes.Folder
                : RepositoryTypes.Sql;
            contentType = dataBuilder.ContentType.Create(appId: AppId, id: 0, name: typeName, nameId: null!, scope: null!, repositoryType: newTypeRepoType);
        }

        var targetEntity = dataBuilder.Entity.Create(
            appId: AppId,
            guid: guid,
            contentType: contentType,
            isPublished: isPublished != "False",
            attributes: finalAttributes.ToImmutableInvIgnoreCase(),
            metadataFor: mdTarget
        );

        return wrap.Return(targetEntity, $"returning {guid} of type {globalTypeIfFound?.Name ?? typeName} with attribs:{finalAttributes.Count} and metadata: {mdTarget != null!}");
    }

    ///// <summary>
    ///// Either add the node to the import list with the dimensions, 
    ///// OR if it is already in the queue, add the dimension information
    ///// </summary>
    ///// <param name="sourceValueNode"></param>
    ///// <param name="tempTargetValues"></param>
    ///// <param name="envLang"></param>
    ///// <param name="readOnly"></param>
    //private void AddNodeToImportListOrEnhancePrevious(XElement sourceValueNode, List<XmlValueToImport> tempTargetValues, TargetLanguageToSourceLanguage envLang, bool readOnly)
    //{
    //    var l = Log.Fn();
    //    var logText = "";
    //    var dimensionsToAdd = new List<ILanguage>();
    //    if (EnvLangs.Single(p => p.Matches(envLang.EnvironmentKey)).DimensionId > 0)
    //    {
    //        dimensionsToAdd.Add(new Language(envLang.EnvironmentKey, readOnly));
    //        logText += "built dimension-list";
    //    }

    //    // If value has already been added to the list, add just dimension with original ReadOnly state
    //    var existingImportValue = tempTargetValues.FirstOrDefault(p => p.XmlValue == sourceValueNode);
    //    if (existingImportValue != null)
    //    {
    //        existingImportValue.Dimensions.AddRange(dimensionsToAdd);
    //        logText += "targetNode already used for another node, just added dimension";
    //    }
    //    else
    //    {
    //        tempTargetValues.Add(new()
    //        {
    //            Dimensions = dimensionsToAdd,
    //            XmlValue = sourceValueNode
    //        });
    //        logText += "targetNode was not used yet, added it";
    //    }

    //    l.Done(logText);
    //}

    //private XElement GetFallbackAttributeInXml(ICollection<XElement> xmlValuesOfAttrib)
    //{
    //    var l = Log.Fn<XElement>();
    //    // First, try to take a fallback node without language assignments
    //    var sourceValueNode = xmlValuesOfAttrib
    //        .FirstOrDefault(xv =>
    //        {
    //            var dimNodes = xv.Elements(XmlConstants.ValueDimNode).ToListOpt();
    //            // keep it if it has no dimensions, or if it has a dimensionId of 0
    //            return !dimNodes.Any() || dimNodes.Any(x => x.Attribute(XmlConstants.DimId)?.Value == "0");
    //        });


    //    // Still nothing found, just take the first one, no matter what's language it's for
    //    // this should probably never happen, but just in case...
    //    if (sourceValueNode == null)
    //    {
    //        l.W("node still null - this indicates a problem! will just use first match");
    //        sourceValueNode = xmlValuesOfAttrib.First();
    //    }
    //    return l.Return(sourceValueNode);
    //}

    //private (XElement? Element, bool ReadOnly) FindAttribWithLanguageMatch(TargetLanguageToSourceLanguage envLang, ICollection<XElement> xmlValuesOfAttrib)
    //{
    //    var l = Log.Fn<(XElement? Element, bool ReadOnly)>(envLang.EnvironmentKey);
    //    XElement? sourceValueNode = null;
    //    var readOnly = false;

    //    // find the xml-node which best matches the language we want to fill in
    //    foreach (var sourceLanguage in envLang.PrioritizedDimensions)
    //    {
    //        var dimensionId = sourceLanguage.DimensionId.ToString();
    //        // find a possible match for exactly this language
    //        sourceValueNode = xmlValuesOfAttrib
    //            .FirstOrDefault(p =>
    //                p.Elements(XmlConstants.ValueDimNode)
    //                    .Any(d => d.Attribute(XmlConstants.DimId)?.Value == dimensionId)
    //            );
    //        if (sourceValueNode == null)
    //            continue;

    //        // if match found, check what the read/write should be
    //        var textVal = sourceValueNode
    //            .Elements(XmlConstants.ValueDimNode)
    //            .FirstOrDefault(p => p.Attribute(XmlConstants.DimId)?.Value == dimensionId)?
    //            .Attribute("ReadOnly")?.Value ?? "false";

    //        readOnly = bool.Parse(textVal);

    //        l.A($"node for {envLang.EnvironmentKey} on Dim:{sourceLanguage.DimensionId}; readOnly: {readOnly}");
    //        break;
    //    }

    //    return l.Return((sourceValueNode, readOnly), (sourceValueNode != null).ToString());
    //}
}