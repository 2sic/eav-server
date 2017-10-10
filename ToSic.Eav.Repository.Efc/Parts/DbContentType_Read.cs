using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbContentType: BllCommandBase
    {
        public DbContentType(DbDataController cntx) : base(cntx, "Db.Type") {}


        private ToSicEavAttributeSets GetTypeByStaticName(string staticName)
        {
            return DbContext.SqlDb.ToSicEavAttributeSets.FirstOrDefault(a =>
                a.AppId == DbContext.AppId && a.StaticName == staticName && a.ChangeLogDeleted == null
                );
        }
        
        //2017-10-10 2dm - don't seem to need this any more, try to get the data from the Attribute.Items...
        ///// <summary>
        ///// Returns the configuration for a content type
        ///// </summary>
        //public IEnumerable<Tuple<IAttributeDefinition, Dictionary<string, IEntity>>> GetTypeConfiguration(string contentTypeStaticName)
        //{
        //    var cache = DataSource.GetCache(null, DbContext.AppId);
        //    var result = (ContentType)cache.GetContentType(contentTypeStaticName);

        //    if (result == null)
        //        throw new Exception("Content type " + contentTypeStaticName + " not found.");

        //    // Resolve ZoneId & AppId of the MetaData. If this AttributeSet uses configuration of another AttributeSet, use MetaData-ZoneId & -AppId
        //    var metaDataAppId = result.ParentAppId;
        //    var metaDataZoneId = result.ParentZoneId;

        //    var metaDataSource = DataSource.GetMetaDataSource(metaDataZoneId, metaDataAppId);

        //    var config = result.Attributes.Select(a => new
        //    {
        //        Attribute = a,
        //        Metadata = metaDataSource
        //            .GetMetadata(Constants.MetadataForAttribute, a.AttributeId)
        //            .ToDictionary(e => e.Type.StaticName.TrimStart('@'), e => e)
        //    });

        //    return config.Select(a => new Tuple<IAttributeDefinition, Dictionary<string, IEntity>>(a.Attribute, a.Metadata));
        //}
        
    }
}
