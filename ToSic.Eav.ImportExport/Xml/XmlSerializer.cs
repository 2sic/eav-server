using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
//using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport;
using ToSic.Eav.Interfaces;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.Xml
{
    public class XmlSerializer: SerializerBase
    {


        public Dictionary<int, XElement> XmlEntities(List<int> entityIds) => entityIds.ToDictionary(e => e, XmlEntity);
        public Dictionary<int, XElement> XmlEntities(List<IEntity> entities) => entities.ToDictionary(e => e.EntityId, XmlEntity);



        protected override string SerializeOne(IEntity entity) => XmlEntity(entity).ToString();

        public XElement XmlEntity(int entityId) => XmlEntity(Lookup(entityId));


        public XElement XmlEntity(IEntity entity)
        {
            var valuesXElement = entity.Attributes.Values
                .Where(a => a.Type != "Entity" || ((a.Values.FirstOrDefault() as IValue<EntityRelationship>)?.TypedContents?.Any() ?? false))
                .OrderBy(a => a.Name)
                .SelectMany(a => a.Values.Select(v => XmlValue(a, v)));

            // create Entity-XElement
            var entityXElement = new XElement(XmlConstants.Entity,
                new XAttribute(XmlConstants.KeyTargetType, App.GetMetadataType(entity.Metadata.TargetType)),
                new XAttribute(XmlConstants.AttSetStatic, entity.Type.StaticName),
                new XAttribute(XmlConstants.AttSetNiceName, entity.Type.Name),
                new XAttribute(XmlConstants.GuidNode, entity.EntityGuid),
                valuesXElement);

            // try to add keys - moved to here from xml-exporter
            if (entity.Metadata.KeyGuid.HasValue)
                entityXElement.Add(new XAttribute(XmlConstants.KeyGuid, entity.Metadata.KeyGuid));
            if (entity.Metadata.KeyNumber.HasValue)
                entityXElement.Add(new XAttribute(XmlConstants.KeyNumber, entity.Metadata.KeyNumber));
            if (!string.IsNullOrEmpty(entity.Metadata.KeyString))
                entityXElement.Add(new XAttribute(XmlConstants.KeyString, entity.Metadata.KeyString));


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
                new XAttribute(XmlConstants.EntityTypeAttribute, attrib.Type),
                ((Value) value).Languages
                .OrderBy(l => l.Key)
                .Select(p => new XElement(XmlConstants.ValueDimNode,
                    new XAttribute(XmlConstants.DimId, p.DimensionId),
                    new XAttribute(XmlConstants.ValueDimRoAttr, p.ReadOnly)
                ))
            );

            return valueXElement;
        }

    }

}
