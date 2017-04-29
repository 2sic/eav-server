using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Practices.ObjectBuilder2;
using ToSic.Eav.ImportExport;
using ToSic.Eav.Persistence.Efc.Models;

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
            var assignmentObjectTypeName = DbContext.SqlDb.ToSicEavAssignmentObjectTypes
                .Single(at => at.AssignmentObjectTypeId == entity.AssignmentObjectTypeId).Name; 
            var attributeSet = GetAttributeSetDefinitionFromCache(entity.AttributeSetId);

            var dbValues = DbContext.SqlDb.ToSicEavValues
                .Include(v => v.Attribute)
                .ThenInclude(a => a.TypeNavigation)
                .Include(v => v.ToSicEavValuesDimensions)
                .ThenInclude(d => d.Dimension)
                .Where(v => v.EntityId == entityId && !v.ChangeLogDeleted.HasValue)
                .ToList();
            var values = dbValues
                .Select(e => new {Key = e.Attribute.StaticName, e.Attribute.TypeNavigation.Type, e.Value, Dimensions = e.ToSicEavValuesDimensions });
            var valuesXElement = values.Select(v => XmlValue(v.Key, v.Value, v.Type, v.Dimensions));

            var relationships = DbContext.Relationships.GetRelationshipsOfParent(entityId)
                .GroupBy(r => r.Attribute.StaticName)
                .Select(r => new {
                    r.Key,
                    Value = r.Select(x => x.ChildEntity?.EntityGuid.ToString() ?? Constants.EmptyRelationship).JoinStrings(",")
                }).ToList();
            var relsXElement = relationships.Select(r => XmlValue(r.Key, r.Value, "Entity", null));

            // create Entity-XElement
            var entityXElement = new XElement("Entity",
                new XAttribute("AssignmentObjectType", assignmentObjectTypeName), 
                new XAttribute("AttributeSetStaticName", attributeSet.StaticName),
                new XAttribute("AttributeSetName", attributeSet.Name),
                new XAttribute("EntityGUID", entity.EntityGuid),
                valuesXElement, relsXElement);

            // try to add keys - moved to here from xml-exporter
            if (entity.KeyGuid.HasValue)
                entityXElement.Add(new XAttribute(XmlConstants.KeyGuid/* "KeyGuid"*/, entity.KeyGuid));
            if (entity.KeyNumber.HasValue)
                entityXElement.Add(new XAttribute(XmlConstants.KeyNumber/*"KeyNumber"*/, entity.KeyNumber));
            if (!string.IsNullOrEmpty(entity.KeyString))
                entityXElement.Add(new XAttribute(XmlConstants.KeyString/*"KeyString"*/, entity.KeyString));


            return entityXElement;
        }

	    private ToSicEavAttributeSets GetAttributeSetDefinitionFromCache(int attributeSetId)
	    {
	        if (!_attrSetCache.ContainsKey(attributeSetId))
	            _attrSetCache[attributeSetId] = DbContext.AttribSet.GetAttributeSet(attributeSetId);
	        var attributeSet = _attrSetCache[attributeSetId];
	        return attributeSet;
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
            var valueXElement = new XElement(XmlConstants.ValueNode,// "Value",
                new XAttribute(XmlConstants.ValueKeyAttr /*"Key"*/, attributeStaticname),
                new XAttribute(XmlConstants.ValueValueAttr/*"Value"*/, valueSerialized),
                !String.IsNullOrEmpty(attributeType) ? new XAttribute("Type", attributeType) : null,
                dimensions?.Select(p => new XElement(XmlConstants.ValueDimNode/*"Dimension"*/,
                        new XAttribute(XmlConstants.DimId/* "DimensionId"*/, p.DimensionId),
                        new XAttribute(XmlConstants.ValueDimRoAttr/* "ReadOnly"*/, p.ReadOnly)
                    ))
                );

            return valueXElement;
        }

    }
}
