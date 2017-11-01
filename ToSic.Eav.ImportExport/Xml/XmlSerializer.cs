﻿using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport;
using ToSic.Eav.Interfaces;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.Xml
{
    public class XmlSerializer: SerializerBase
    {

        public Dictionary<int, XElement> ToXml(List<int> entityIds) => entityIds.ToDictionary(e => e, ToXml);
        public Dictionary<int, XElement> ToXml(List<IEntity> entities) => entities.ToDictionary(e => e.EntityId, ToXml);
        public XElement ToXml(int entityId) => ToXml(Lookup(entityId));


        public override string Serialize(IEntity entity) => ToXml(entity).ToString();


        public XElement ToXml(IEntity entity)
        {
            var valuesXElement = entity.Attributes.Values
                .Where(a => a.Type != "Entity" || ((a.Values.FirstOrDefault() as IValue<EntityRelationship>)?.TypedContents?.Any() ?? false))
                .OrderBy(a => a.Name)
                .SelectMany(a => a.Values.Select(v => XmlValue(a, v)));

            // create Entity-XElement
            var entityXElement = new XElement(XmlConstants.Entity,
                new XAttribute(XmlConstants.KeyTargetType, Factory.Resolve<IGlobalMetadataProvider>().GetType(entity.MetadataFor.TargetType)),
                new XAttribute(XmlConstants.AttSetStatic, entity.Type.StaticName),
                new XAttribute(XmlConstants.AttSetNiceName, entity.Type.Name),
                new XAttribute(XmlConstants.GuidNode, entity.EntityGuid),
                valuesXElement);

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
