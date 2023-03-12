using System;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Catalog;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using static ToSic.Eav.DataSources.DataSourceConstants;

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
        public string DataSourceTypeIdentifier => _dstName ?? (_dstName = DataSourceCatalog.Find(GetCorrectedTypeName()));
        private string _dstName;

        public Type DataSourceType => _dsType ?? (_dsType = DataSourceCatalog.FindTypeByGuidOrName(GetCorrectedTypeName()));
        private Type _dsType;
        
        private string DataSourceTypeInConfig
            => Get<string>(QueryConstants.PartAssemblyAndType, null)
               ?? throw new Exception("Tried to get DataSource Type of a query part, but didn't find anything");



        /// <summary>
        /// Check if a Query part has an old assembly name, and if yes, correct it to the new name
        /// </summary>
        /// <returns></returns>
        private string GetCorrectedTypeName()
        {
            var assemblyAndType = DataSourceTypeInConfig;
            // Correct old stored names (ca. before 2sxc 4 to new)
            var newName = assemblyAndType.EndsWith(V3To4DataSourceDllOld)
                ? assemblyAndType.Replace(V3To4DataSourceDllOld, V3To4DataSourceDllNew)
                : assemblyAndType;
            return newName;
        }
    }
}
