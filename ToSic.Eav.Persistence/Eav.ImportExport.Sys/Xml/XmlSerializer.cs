using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Serialization.Sys;


namespace ToSic.Eav.ImportExport.Sys.Xml;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class XmlSerializer(SerializerBase.Dependencies services) : SerializerBase(services, "IEx.XmlSer")
{
    [field: AllowNull, MaybeNull]
    private Dictionary<string, int> Dimensions
    {
        get => field ?? throw new ArgumentNullException(nameof(Dimensions));
        set;
    }

    public XmlSerializer Init(Dictionary<string, int> dimensionMapping, IAppReader appReader)
    {
        Initialize(appReader);
        Dimensions = dimensionMapping;
        return this;
    }

    public XElement ToXml(int entityId) => ToXml(Lookup(entityId) ?? throw new NullReferenceException($"Tried to convert entity {entityId} to XML but could not find entity"));


    public override string Serialize(IEntity entity) => ToXml(entity).ToString();


    public XElement ToXml(IEntity entity)
    {
        // 2026-02-09 2dm - simplified code, but removed something which was there before, but probably not relevant
        var valuesXElement = entity.Attributes.Values
            //.Where(a => a.Type != ValueTypes.Entity || ((a.Values.FirstOrDefault() as IValue<IEnumerable<IEntity>>)?.TypedContents?.Any() ?? false))
            .Where(a => !a.IsEntity())
            .OrderBy(a => a.Name)
            .SelectMany(a => a.Values.Select(v => XmlValue(a, v)));

        // create Entity-XElement
        var entityXElement = new XElement(XmlConstants.Entity,
            // #TargetTypeIdInsteadOfTarget
            new XAttribute(XmlConstants.KeyTargetType, entity.MetadataFor.TargetType),
            new XAttribute(XmlConstants.KeyTargetTypeNameOld, MetadataTargets.GetName(entity.MetadataFor.TargetType)),
            new XAttribute(XmlConstants.AttSetStatic, entity.Type.NameId),
            new XAttribute(XmlConstants.AttSetNiceName, entity.Type.Name),
            new XAttribute(XmlConstants.GuidNode, entity.EntityGuid),
            valuesXElement);

        if (!entity.IsPublished)
            entityXElement.Add(new XAttribute(XmlConstants.IsPublished, "False"));

        // experimental new in V11
        if (entity.Type.RepositoryType != RepositoryTypes.Sql)
            entityXElement.Add(new XAttribute(XmlConstants.EntityIsJsonAttribute, "True"));

        // try to add keys - moved to here from xml-exporter
        if (entity.MetadataFor.KeyGuid.HasValue)
            entityXElement.Add(new XAttribute(XmlConstants.KeyGuid, entity.MetadataFor.KeyGuid));
        if (entity.MetadataFor.KeyNumber.HasValue)
            entityXElement.Add(new XAttribute(XmlConstants.KeyNumber, entity.MetadataFor.KeyNumber));
        if (!string.IsNullOrEmpty(entity.MetadataFor.KeyString))
            entityXElement.Add(new XAttribute(XmlConstants.KeyString, entity.MetadataFor.KeyString!));


        return entityXElement;
    }


    /// <summary>
    /// Generate an xml-node containing a value, 
    /// plus optionally sub-nodes describing the dimensions / relationships inside
    /// </summary>
    /// <returns></returns>
    private XElement XmlValue(IAttribute attrib, IValue value)
    {
        var str = value.Serialized ?? "";

        //var valueSerialized = value.Serialized;
        // create Value-Child-Element with Dimensions as Children
        var valueXElement = new XElement(XmlConstants.ValueNode,
            new XAttribute(XmlConstants.KeyAttr, attrib.Name),
            new XAttribute(XmlConstants.ValueAttr, str),
            new XAttribute(XmlConstants.EntityTypeAttribute, attrib.Type.ToString()),
            value.Languages
                .OrderBy(l => l.Key)
                .Select(p => new XElement(XmlConstants.ValueDimNode,
                    // because JSON-Entities do not contain valid dimension ids, lookup id
                    new XAttribute(XmlConstants.DimId, p.DimensionId == 0 ? (Dimensions.ContainsKey(p.Key.ToLowerInvariant()) ? Dimensions[p.Key] : 0) : p.DimensionId),
                    new XAttribute(XmlConstants.ValueDimRoAttr, p.ReadOnly)
                ))
        );

        return valueXElement;
    }

    //private XElement XmlJsonEntity(IEntity entity)
    //{
    //    var jsonElement = new XElement(XmlConstants.JsonEntityNode);
    //    return jsonElement;
    //}
}