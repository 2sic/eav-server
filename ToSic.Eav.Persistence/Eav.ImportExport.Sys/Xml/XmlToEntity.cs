using System.Xml.Linq;
using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Data.Sys.Global;
using ToSic.Eav.Data.Sys.Values;
using ToSic.Eav.Metadata.Targets;

namespace ToSic.Eav.ImportExport.Sys.Xml;


/// <summary>
/// Import EAV Data from XML Format
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class XmlToEntity(IGlobalDataService globalData, DataAssembler dataAssembler, ContentTypeAssemblyKit ctAssemblyKit)
    : ServiceWithSetup<XmlToEntity.Options>("Imp.XmlEnt", connect: [globalData, dataAssembler, ctAssemblyKit])
{
    /// <summary>
    /// Options
    /// </summary>
    /// <param name="AppId">Target AppId</param>
    /// <param name="EnvDefLang">Default environment language</param>
    /// <param name="EnvLangs">All environment languages, properly sorted</param>
    /// <param name="LogSettings">Log settings for the import process</param>
    public record Options(int AppId, string EnvDefLang, ICollection<TargetLanguageToSourceLanguage> EnvLangs, LogSettings LogSettings);

    /// <summary>
    /// Returns an EAV import entity
    /// </summary>
    /// <param name="xEntity">xEntity to parse</param>
    /// <param name="mdTarget"></param>
    internal Entity BuildEntityFromXml(XElement xEntity, Target mdTarget)
    {
        var logDetails = Log.IfDetails(MyOptions.LogSettings);

        var l = logDetails.Fn<Entity>();

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

        var xmlValueListHelper = new XmlValueListHelper(MyOptions, Log);

        // Process each attribute (values grouped by StaticName)
        var finalAttributes = valuesGroupedByStaticName
            .ToDictionary(
                keySelector: g => g.StaticName,
                elementSelector: sourceAttrib =>
                {
                    var lAttrib = logDetails.Fn<IAttribute>(sourceAttrib.StaticName);

                    // Process each target's language
                    var tempTargetValues = xmlValueListHelper.CreateList(sourceAttrib.Values);

                    // construct value elements
                    var currentAttributesImportValues = tempTargetValues
                        .Select(xVal =>
                        {
                            var vType = xVal.XmlValue.Attribute(XmlConstants.EntityTypeAttribute)?.Value
                                        ?? throw new NullReferenceException("can't build attribute with unknown value-type");
                            var vValue = xVal.XmlValue.Attribute(XmlConstants.ValueAttr)?.Value
                                         ?? throw new NullReferenceException("can't build attribute without value");
                            return dataAssembler.Value.Create(ValueTypeHelpers.Get(vType), vValue, xVal.Dimensions.ToImmutableSafe());
                        })
                        .ToListOpt();

                    // construct the attribute with these value elements
                    var value = ValueTypeHelpers.Get(tempTargetValues.First().XmlValue.Attribute(XmlConstants.EntityTypeAttribute)?.Value ?? "");
                    var newAttr = dataAssembler.Attribute.Create(sourceAttrib.StaticName, value, currentAttributesImportValues);

                    // attach to attributes-list
                    return lAttrib.Return(newAttr);
                });

        var typeName = xEntity.Attribute(XmlConstants.AttSetStatic)?.Value;
        if (typeName == null)
            throw new NullReferenceException("trying to import an xml entity but type is null - " + xEntity);
		    
        // find out if it's a system type, and use that if it exists
        var guidString = xEntity.Attribute(XmlConstants.GuidNode)?.Value
                         ?? throw new NullReferenceException("can't import an entity without a guid identifier");
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
            contentType = ctAssemblyKit.Type.Create(appId: MyOptions.AppId, id: 0, name: typeName, nameId: null!, scope: null!, repositoryType: newTypeRepoType);
        }

        var targetEntity = dataAssembler.Entity.Create(
            appId: MyOptions.AppId,
            guid: guid,
            contentType: contentType,
            isPublished: isPublished != "False",
            attributes: finalAttributes.ToImmutableInvIgnoreCase(),
            metadataFor: mdTarget
        );

        return l.Return(targetEntity, $"returning {guid} of type {globalTypeIfFound?.Name ?? typeName} with attribs:{finalAttributes.Count} and metadata: {mdTarget != null!}");
    }

}