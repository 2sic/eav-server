using System;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Catalog;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.DataSources.Queries
{
    /// <summary>
    /// The configuration / definition of a query part. The <see cref="QueryDefinition"/> uses a bunch of these together to build a query. 
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public class QueryPartDefinition: EntityBasedWithLog
    {
        [PrivateApi]
        public QueryPartDefinition(IEntity entity, ILog parentLog) : base(entity, parentLog, "DS.QrPart")
        {

        }

        /// <summary>
        /// Information for this part, how it's to be displayed in the visual query.
        /// This is a JSON string containing positioning etc.
        /// </summary>
        public string VisualDesignerData => Get(QueryConstants.VisualDesignerData, "");

        /// <summary>
        /// The .net type which the data source has for this part. <br/>
        /// Will automatically resolve old names to new names as specified in the DataSources <see cref="VisualQueryAttribute"/>
        /// </summary>
        public string DataSourceType => _dataSourceType ?? (_dataSourceType = RewriteOldAssemblyNames(DataSourceTypeInConfig));
        private string _dataSourceType;
        
        private string DataSourceTypeInConfig
            => Get<string>(QueryConstants.PartAssemblyAndType, null)
               ?? throw new Exception("Tried to get DataSource Type of a query part, but didn't find anything");


        /// <summary>
        /// Check if a Query part has an old assembly name, and if yes, correct it to the new name
        /// </summary>
        /// <param name="assemblyAndType"></param>
        /// <returns></returns>
        private string RewriteOldAssemblyNames(string assemblyAndType)
        {
            var newName = assemblyAndType.EndsWith(DataSourceConstants.V3To4DataSourceDllOld)
                ? assemblyAndType.Replace(DataSourceConstants.V3To4DataSourceDllOld, DataSourceConstants.V3To4DataSourceDllNew)
                : assemblyAndType;

            // find the new name in the catalog
            return new DataSourceCatalog(Log).Find(newName);
        }


    }
}
