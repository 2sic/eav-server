using System;
using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Get Parent Entities (parent-relationships) of the Entities coming into this DataSource
    /// </summary>
    /// <remarks>
    /// Added in v12.10
    /// </remarks>
    [VisualQuery(
        NiceName = "Parents",
        UiHint = "Get the item's parents",
        Icon = "family_restroom",
        Type = DataSourceType.Lookup,
        GlobalName = "ToSic.Eav.DataSources.Parents, ToSic.Eav.DataSources",
        In = new[] { Constants.DefaultStreamNameRequired },
        DynamicOut = false,
        ExpectsDataOfType = "a72cb2f4-52bb-41e6-9281-10e69aeb0310",
        HelpLink = "https://r.2sxc.org/DsParents")]
    [InternalApi_DoNotUse_MayChangeWithoutNotice("WIP")]
    public class Parents : RelationshipDataSourceBase
    {
        /// <inheritdoc/>
        [PrivateApi]
        public override string LogId => $"{DataSourceConstants.LogPrefix}.Parent";

        /// <summary>
        /// Name of the field (in the parent) pointing to the child.
        /// If left blank, will use get all children.
        ///
        /// Example: If a person is referenced by books as both `Author` and `Illustrator` then leaving this empty will get both relationships, but specifying `Author` will only get this person if it's the author. 
        /// </summary>
        public override string FieldName
        {
            get => Configuration[nameof(FieldName)];
            set => Configuration[nameof(FieldName)] = value;
        }

        /// <summary>
        /// Name of the content-type to get.
        /// Will only get parents of the specified type.
        ///
        /// Example: If a person is referenced by books (as author) as by companies) as employee, then you may want to only find companies referencing this book. 
        /// </summary>
        public override string ContentTypeName
        {
            get => Configuration[nameof(ContentTypeName)];
            set => Configuration[nameof(ContentTypeName)] = value;
        }


        /// <summary>
        /// Construct function for the get of the related items
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        protected override Func<IEntity, IEnumerable<IEntity>> InnerGet(string fieldName, string typeName) 
            => o => o.Relationships.FindParents(typeName, fieldName, Log);
    }
}
