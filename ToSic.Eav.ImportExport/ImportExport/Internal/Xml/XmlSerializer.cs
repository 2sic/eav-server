using System.Xml.Linq;
using ToSic.Eav.Repositories;
using ToSic.Eav.Serialization.Internal;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.ImportExport.Internal.Xml;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class XmlSerializer(SerializerBase.MyServices services) : SerializerBase(services, "IEx.XmlSer")
{
    private Dictionary<string, int> _dimensions;

    public XmlSerializer Init(Dictionary<string, int> dimensionMapping, IAppState appState)
    {
        Initialize(appState);
        _dimensions = dimensionMapping;
        return this;
    }

    public XElement ToXml(int entityId) => ToXml(Lookup(entityId));


    public override string Serialize(IEntity entity) => ToXml(entity).ToString();


    public XElement ToXml(IEntity entity)
    {
        var valuesXElement = entity.Attributes.Values
            .Where(a => a.Type != ValueTypes.Entity || ((a.Values.FirstOrDefault() as IValue<IEnumerable<IEntity>>)?.TypedContents?.Any() ?? false))
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

        // experimental new in V11
        if(entity.Type.RepositoryType != RepositoryTypes.Sql)
            entityXElement.Add(new XAttribute(XmlConstants.EntityIsJsonAttribute, "True"));

        // try to add keys - moved to here from xml-exporter
        if (entity.MetadataFor.KeyGuid.HasValue)
            entityXElement.Add(new XAttribute(XmlConstants.KeyGuid, entity.MetadataFor.KeyGuid));
        if (entity.MetadataFor.KeyNumber.HasValue)
            entityXElement.Add(new XAttribute(XmlConstants.KeyNumber, entity.MetadataFor.KeyNumber));
        if (!string.IsNullOrEmpty(entity.MetadataFor.KeyString))
            entityXElement.Add(new XAttribute(XmlConstants.KeyString, entity.MetadataFor.KeyString));


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
                    new XAttribute(XmlConstants.DimId, p.DimensionId == 0 ? (_dimensions.ContainsKey(p.Key.ToLowerInvariant()) ? _dimensions[p.Key] : 0) : p.DimensionId),
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