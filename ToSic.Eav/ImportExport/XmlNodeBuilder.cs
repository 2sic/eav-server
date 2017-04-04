using System;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.BLL;

namespace ToSic.Eav.ImportExport
{
	/// <summary>
	/// Export EAV Data in XML Format
	/// </summary>
	public class XmlNodeBuilder: BllCommandBase
	{
        public XmlNodeBuilder(EavDataController c) : base(c) { }

        /// <summary>
        /// Returns an Entity XElement
        /// </summary>
        public XElement GetEntityXElement(int entityId)
        {
            var cache = DataSource.GetCache(Context.ZoneId, Context.AppId);
            var iEntity = cache.List[entityId];
            return GetEntityXElement(iEntity);
        }

        /// <summary>
        /// Returns an Entity XElement
        /// Works, but does not export the entity relationships
        /// </summary>
        public XElement GetEntityXElementUncached(int entityId)
        {
            //var cache = DataSource.GetCache(Context.ZoneId, Context.AppId);
            //var iEntity = cache.List[entityId];
            var iEntity = new DbLoadIntoEavDataStructure(Context).GetEavEntity(entityId);

            return GetEntityXElement(iEntity);
        }

        /// <summary>
        /// Returns an Entity XElement
        /// </summary>
        private XElement GetEntityXElement(IEntity entity)
		{
			var eavEntity = Context.Entities.GetEntity(entity.EntityId);
			//var attributeSet = _ctx.GetAttributeSet(eavEntity.AttributeSetID);
            
			// Prepare Values
			var values = (from e in entity.Attributes
						  where e.Value.Values != null
						  select new
						  {
							  allValues = from v in e.Value.Values
										  select new
										  {
											  e.Key,
											  e.Value.Type,
											  ValueModel = v
										  }
						  }).SelectMany(e => e.allValues);

			var valuesXElement = from v in values
								 select GetValueXElement(v.Key, v.ValueModel, v.Type);

			// create Entity-XElement
			return new XElement("Entity",
				new XAttribute("AssignmentObjectType", eavEntity.AssignmentObjectType.Name),
				new XAttribute("AttributeSetStaticName", entity.Type.StaticName),
				new XAttribute("AttributeSetName", entity.Type.Name),
				new XAttribute("EntityGUID", entity.EntityGuid),
				valuesXElement);
		}

		/// <summary>
		/// Gets a Value XElement
		/// </summary>
		private XElement GetValueXElement(string attributeStaticname, IValue value, string attributeType)
		{
		    var valueSerialized = value.Serialized; 
			// create Value-Child-Element with Dimensions as Children
			var valueXElement = new XElement("Value",
				new XAttribute("Key", attributeStaticname),
				new XAttribute("Value", valueSerialized),
				!String.IsNullOrEmpty(attributeType) ? new XAttribute("Type", attributeType) : null,
				value.Languages.Select(p => new XElement("Dimension",
						new XAttribute("DimensionID", p.DimensionId),
						new XAttribute("ReadOnly", p.ReadOnly)
					))
				);

			return valueXElement;
		}

	}
}
