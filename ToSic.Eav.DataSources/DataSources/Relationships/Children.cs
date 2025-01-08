using ToSic.Eav.Context;
using ToSic.Eav.DataSources.Internal;
using static ToSic.Eav.DataSource.DataSourceConstants;

namespace ToSic.Eav.DataSources;

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
    Icon = DataSourceIcons.ParentChild,
    Type = DataSourceType.Lookup,
    NameId = "9f8de7ee-d1aa-4055-9bf9-8f183259cb05",
    In = [InStreamDefaultRequired],
    DynamicOut = false,
    ConfigurationType = "832cd470-49f2-4909-a08a-77644457713e",
    HelpLink = "https://go.2sxc.org/DsChildren")]
[InternalApi_DoNotUse_MayChangeWithoutNotice("WIP")]

public class Children(DataSourceBase.MyServices services, IContextResolverUserPermissions userPermissions) : RelationshipDataSourceBase(services, userPermissions, $"{DataSourceConstantsInternal.LogPrefix}.Child")
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