using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Practices.ObjectBuilder2;
using ToSic.Eav.Persistence.EFC11.Models;

//using System.Data.Objects.DataClasses;

namespace ToSic.Eav.Repository.Efc.Parts
{
	/// <summary>
	/// Export EAV Data in XML Format
	/// </summary>
	internal class DbXmlBuilder: BllCommandBase
	{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        internal DbXmlBuilder(DbDataController c) : base(c) { }

        /// <summary>
        /// Use as local cache, because often we'll need the same attribute set again and again in an export
        /// </summary>
        private readonly Dictionary<int, ToSicEavAttributeSets> _attrSetCache = new Dictionary<int, ToSicEavAttributeSets>();

        /// <summary>
        /// Returns an Entity XElement
        /// </summary>
        internal XElement XmlEntity(int entityId)
        {
            var entity = DbContext.Entities.GetDbEntity(entityId);
            if(!_attrSetCache.ContainsKey(entity.AttributeSetId))
                _attrSetCache[entity.AttributeSetId] = DbContext.AttribSet.GetAttributeSet(entity.AttributeSetId);
            var assignmentObjectTypeName = entity.AssignmentObjectType.Name;
            var attributeSet = _attrSetCache[entity.AttributeSetId];

            var values = entity.ToSicEavValues.Select(e => new {Key = e.Attribute.StaticName, e.Attribute.TypeNavigation.Type, e.Value, Dimensions = e.ToSicEavValuesDimensions });
            var valuesXElement = values.Select(v => XmlValue(v.Key, v.Value, v.Type, v.Dimensions));

            var relationships = entity.RelationshipsWithThisAsParent/*EntityParentRelationships*/.GroupBy(r => r.Attribute.StaticName)
                    .Select( r => new {
                                r.Key,
                                Value = r.Select(x => x.ChildEntity.EntityGuid.ToString()).JoinStrings(",")
                            });
            var relsXElement = relationships.Select(r => XmlValue(r.Key, r.Value, "Entity", null));

            // create Entity-XElement
            var entityXElement = new XElement("Entity",
                new XAttribute("AssignmentObjectType", assignmentObjectTypeName), 
                new XAttribute("AttributeSetStaticName", attributeSet.StaticName),
                new XAttribute("AttributeSetName", attributeSet.Name),
                new XAttribute("EntityGuid", entity.EntityGuid),
                valuesXElement, relsXElement);

            // try to add keys - moved to here from xml-exporter
            if (entity.KeyGuid.HasValue)
                entityXElement.Add(new XAttribute("KeyGuid", entity.KeyGuid));
            if (entity.KeyNumber.HasValue)
                entityXElement.Add(new XAttribute("KeyNumber", entity.KeyNumber));
            if (!string.IsNullOrEmpty(entity.KeyString))
                entityXElement.Add(new XAttribute("KeyString", entity.KeyString));


            return entityXElement;
        }


        /// <summary>
        /// Generate an xml-node containing a value, 
        /// plus optionally sub-nodes describing the dimensions / relationships inside
        /// </summary>
        /// <param name="attributeStaticname"></param>
        /// <param name="valueSerialized"></param>
        /// <param name="attributeType"></param>
        /// <param name="dimensions"></param>
        /// <returns></returns>
        private XElement XmlValue(string attributeStaticname, string valueSerialized, string attributeType, ICollection<ToSicEavValuesDimensions> dimensions )
        {
            //var valueSerialized = value.Serialized;
            // create Value-Child-Element with Dimensions as Children
            var valueXElement = new XElement("Value",
                new XAttribute("Key", attributeStaticname),
                new XAttribute("Value", valueSerialized),
                !String.IsNullOrEmpty(attributeType) ? new XAttribute("Type", attributeType) : null,
                dimensions?.Select(p => new XElement("Dimension",
                        new XAttribute("DimensionId", p.DimensionId),
                        new XAttribute("ReadOnly", p.ReadOnly)
                    ))
                );

            return valueXElement;
        }

    }
}
