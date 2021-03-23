using System;
using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Catalog;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources.Queries
{
    /// <summary>
    /// The configuration / definition of a query part. The <see cref="QueryDefinition"/> uses a bunch of these together to build a query. 
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public class QueryPartDefinition: EntityBasedType
    {
        [PrivateApi]
        public QueryPartDefinition(IEntity entity) : base(entity)
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
        public string DataSourceType => RewriteOldAssemblyNames(DataSourceTypeInConfig);

        private string DataSourceTypeInConfig
            => Get<string>(QueryConstants.PartAssemblyAndType, null)
               ?? throw new Exception("Tried to get DataSource Type of a query part, but didn't find anything");


        /// <summary>
        /// Check if a Query part has an old assembly name, and if yes, correct it to the new name
        /// </summary>
        /// <param name="assemblyAndType"></param>
        /// <returns></returns>
        private static string RewriteOldAssemblyNames(string assemblyAndType)
        {
            var newName = assemblyAndType.EndsWith(Constants.V3To4DataSourceDllOld)
                ? assemblyAndType.Replace(Constants.V3To4DataSourceDllOld, Constants.V3To4DataSourceDllNew)
                : assemblyAndType;

            // find the new name in the catalog
            return CatalogHelpers.FindName(newName);
        }


        //[PrivateApi]
        //public Dictionary<string, object> AsDictionary()
        //{
        //    var attributes = Entity.AsDictionary();

        //    attributes[QueryConstants.VisualDesignerData] = JsonConvert.DeserializeObject(VisualDesignerData);

        //    // Replace ToSic.Eav with ToSic.Eav.DataSources because they moved to a different DLL
        //    attributes[QueryConstants.PartAssemblyAndType] = DataSourceType;

        //    return attributes;
        //}

    }
}
