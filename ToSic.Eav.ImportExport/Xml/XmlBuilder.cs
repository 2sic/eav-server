using System;
using System.Linq;
using System.Xml.Linq;

namespace ToSic.Eav.ImportExport.Xml
{
    public class XmlBuilder
    {


        ///// <summary>
        ///// Returns an Entity XElement
        ///// </summary>
        //public XElement BuildEntityXElement(IEntity entity, string assignmentObjectTypeName, int? keyNumber = null, string keyString = null, Guid? keyGuid = null)
        //{
        //    // initial
        //    //var attributeSet = _ctx.GetAttributeSet(eavEntity.AttributeSetID);
        //    // replacement, before passing it in
        //    //var eavEntity = DbContext.Entities.GetDbEntity(entity.EntityId);
        //    //var assignmentObjectTypeName = eavEntity.AssignmentObjectType.Name;


        //    // Prepare Values
        //    var values = (from e in entity.Attributes
        //                  where e.Value.Values != null
        //                  select new
        //                  {
        //                      allValues = from v in e.Value.Values
        //                                  select new
        //                                  {
        //                                      e.Key,
        //                                      e.Value.Type,
        //                                      ValueModel = v
        //                                  }
        //                  }).SelectMany(e => e.allValues);

        //    var valuesXElement = from v in values
        //                         select GetValueXElement(v.Key, v.ValueModel, v.Type);

        //    // create Entity-XElement
        //    var result = new XElement("Entity",
        //        new XAttribute("AssignmentObjectType", assignmentObjectTypeName),   // todo 2017-04-11 2dm: why is this stored, but not the keys???
        //        new XAttribute("AttributeSetStaticName", entity.Type.StaticName),
        //        new XAttribute("AttributeSetName", entity.Type.Name),
        //        new XAttribute("EntityGUID", entity.EntityGuid),
        //        valuesXElement);

        //    // try to add keys - moved to here from xml-exporter
        //    if (keyGuid.HasValue)
        //        result.Add(new XAttribute("KeyGuid", keyGuid));
        //    if (keyNumber.HasValue)
        //        result.Add(new XAttribute("KeyNumber", keyNumber));
        //    if (!string.IsNullOrEmpty(keyString))
        //        result.Add(new XAttribute("KeyString", keyString));

        //    return result;
        //}

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
