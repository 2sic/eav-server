using System;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.BLL;

namespace ToSic.Eav.ImportExport
{
	/// <summary>
	/// Export EAV Data in XML Format
	/// </summary>
	internal class XmlNodeBuilder: BllCommandBase
	{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        internal XmlNodeBuilder(DbDataController c) : base(c) { }

        /// <summary>
        /// Returns an Entity XElement
        /// </summary>
        internal XElement GetEntityXElement(int entityId)
        {
            //var cache = DataSource.GetCache(DbContext.ZoneId, DbContext.AppId);
            //var iEntity = cache.List[entityId];
            //var eavEntity = DbContext.Entities.GetDbEntity(entityId);
            //var assignmentObjectTypeName = eavEntity.AssignmentObjectType.Name;

            return BuildEntityXElement(entityId);// iEntity, assignmentObjectTypeName);
            //return GetEntityXElement(iEntity, assignmentObjectTypeName);
        }

        /// <summary>
        /// Returns an Entity XElement
        /// Works, but does not export the entity relationships
        /// </summary>
        internal XElement GetEntityXElementUncached(int entityId)
        {
            //var cache = DataSource.GetCache(Context.ZoneId, Context.AppId);
            //var iEntity = cache.List[entityId];
            //var iEntity = new DbLoadIntoEavDataStructure(DbContext).GetEavEntity(entityId);

            //var eavEntity = DbContext.Entities.GetDbEntity(entityId);
            //var assignmentObjectTypeName = eavEntity.AssignmentObjectType.Name;

            return BuildEntityXElement(entityId);// iEntity, assignmentObjectTypeName);
            //return GetEntityXElement(iEntity, assignmentObjectTypeName);
        }

        /// <summary>
        /// Returns an Entity XElement
        /// </summary>
        private XElement BuildEntityXElement(int entityId)//, string assignmentObjectTypeName, int? keyNumber = null, string keyString = null, Guid? keyGuid = null)
        {
            // initial
            //var attributeSet = _ctx.GetAttributeSet(eavEntity.AttributeSetID);
            // replacement, before passing it in
            var entity = DbContext.Entities.GetDbEntity(entityId);
            //var eavEntity = DbContext.Entities.GetDbEntity(entityId);
            var assignmentObjectTypeName = entity.AssignmentObjectType.Name;
            var attributeSet = DbContext.AttribSet.GetAttributeSet(entity.AttributeSetID);
            

            // Prepare Values
            //var valuesTest = (from e in  testEntity.Attributes
            //              where e.Value.Values != null
            //              select new
            //              {
            //                  allValues = from v in e.Value.Values
            //                              select new
            //                              {
            //                                  e.Key,
            //                                  e.Value.Type,
            //                                  ValueModel = v
            //                              }
            //              }).SelectMany(e => e.allValues);

            //var valuesXElement = from v in valuesTest
            //                     select GetValueXElement(v.Key, v.ValueModel, v.Type);

            var values = entity.Values.Select(e => new {Key = e.Attribute.StaticName, Type = e.Attribute.AttributeType.Type, e.Value, Dimensions = e.ValuesDimensions });
            var valuesXElement = values.Select(v => BuildValueXElement(v.Key, v.Value, v.Type, v.Dimensions));


            // create Entity-XElement
            var entityXElement = new XElement("Entity",
                new XAttribute("AssignmentObjectType", assignmentObjectTypeName),   // todo 2017-04-11 2dm: why is this stored, but not the keys???
                new XAttribute("AttributeSetStaticName", attributeSet.StaticName),// entity.Type.StaticName),
                new XAttribute("AttributeSetName", attributeSet.Name),// entity.Type.Name),
                new XAttribute("EntityGUID", entity.EntityGUID),//.EntityGuid),
                valuesXElement);

            // try to add keys - moved to here from xml-exporter
            if (entity.KeyGuid.HasValue)
                entityXElement.Add(new XAttribute("KeyGuid", entity.KeyGuid));
            if (entity.KeyNumber.HasValue)
                entityXElement.Add(new XAttribute("KeyNumber", entity.KeyNumber));
            if (!string.IsNullOrEmpty(entity.KeyString))
                entityXElement.Add(new XAttribute("KeyString", entity.KeyString));


            return entityXElement;
        }



        private XElement BuildValueXElement(string attributeStaticname, string valueSerialized, string attributeType, EntityCollection<ValueDimension> dimensions )
        {
            //var valueSerialized = value.Serialized;
            // create Value-Child-Element with Dimensions as Children
            var valueXElement = new XElement("Value",
                new XAttribute("Key", attributeStaticname),
                new XAttribute("Value", valueSerialized),
                !String.IsNullOrEmpty(attributeType) ? new XAttribute("Type", attributeType) : null,
                dimensions.Select(p => new XElement("Dimension",
                        new XAttribute("DimensionID", p.DimensionID),
                        new XAttribute("ReadOnly", p.ReadOnly)
                    ))
                );

            return valueXElement;
        }

        ///// <summary>
        ///// Gets a Value XElement
        ///// </summary>
        //private XElement GetValueXElement(string attributeStaticname, IValue value, string attributeType)
        //{
        //    var valueSerialized = value.Serialized;
        //    // create Value-Child-Element with Dimensions as Children
        //    var valueXElement = new XElement("Value",
        //        new XAttribute("Key", attributeStaticname),
        //        new XAttribute("Value", valueSerialized),
        //        !String.IsNullOrEmpty(attributeType) ? new XAttribute("Type", attributeType) : null,
        //        value.Languages.Select(p => new XElement("Dimension",
        //                new XAttribute("DimensionID", p.DimensionId),
        //                new XAttribute("ReadOnly", p.ReadOnly)
        //            ))
        //        );

        //    return valueXElement;
        //}

    }
}
