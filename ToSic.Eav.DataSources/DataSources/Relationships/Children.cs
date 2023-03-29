using System;
using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Lib.Documentation;
using static ToSic.Eav.DataSource.DataSourceConstants;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Get Children Entities (child-relationships) of the Entities coming into this DataSource
    /// </summary>
    /// <remarks>
    /// * Added in v12.10
    /// * Changed in v15.05 to use the [immutable convention](xref:NetCode.Conventions.Immutable)
    /// </remarks>
    [VisualQuery(
        NiceName = "Children",
        UiHint = "Get the item's children",
        Icon = Icons.ParentChild,
        Type = DataSourceType.Lookup,
        NameId = "9f8de7ee-d1aa-4055-9bf9-8f183259cb05",
        In = new[] { InStreamDefaultRequired },
        DynamicOut = false,
        ConfigurationType = "832cd470-49f2-4909-a08a-77644457713e",
        HelpLink = "https://r.2sxc.org/DsChildren")]
    [InternalApi_DoNotUse_MayChangeWithoutNotice("WIP")]

    public class Children : RelationshipDataSourceBase
    {
        /// <summary>
        /// Name of the field pointing to the children.
        /// If left blank, will use get all children.
        /// </summary>
        public override string FieldName => Configuration.GetThis();

        /// <summary>
        /// Name of the content-type to get. 
        /// If specified, would only keep the children of this content-type.
        ///
        /// Can usually be left empty (recommended).
        /// </summary>
        public override string ContentTypeName => Configuration.GetThis();

        public Children(MyServices services) : base(services, $"{LogPrefix}.Child")
        {
        }

        /// <summary>
        /// Construct function for the get of the related items
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        [PrivateApi]
        protected override Func<IEntity, IEnumerable<IEntity>> InnerGet(string fieldName, string typeName) 
            => o => o.Relationships.FindChildren(fieldName, typeName, Log);

    }
}
