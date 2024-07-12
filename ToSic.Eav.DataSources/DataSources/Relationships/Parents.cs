using ToSic.Eav.Context;
using ToSic.Eav.DataSources.Internal;
using static ToSic.Eav.DataSource.Internal.DataSourceConstants;

namespace ToSic.Eav.DataSources;

/// <summary>
/// Get Parent Entities (parent-relationships) of the Entities coming into this DataSource
/// </summary>
/// <remarks>
/// * Added in v12.10
/// * Changed in v15.05 to use the [immutable convention](xref:NetCode.Conventions.Immutable)
/// </remarks>
[VisualQuery(
    NiceName = "Parents",
    UiHint = "Get the item's parents",
    Icon = DataSourceIcons.Parents,
    Type = DataSourceType.Lookup,
    NameId = "915217e5-7957-4303-a19c-a15505f2ad1d",
    In = [InStreamDefaultRequired],
    DynamicOut = false,
    ConfigurationType = "a72cb2f4-52bb-41e6-9281-10e69aeb0310",
    HelpLink = "https://go.2sxc.org/DsParents")]
[InternalApi_DoNotUse_MayChangeWithoutNotice("WIP")]
public class Parents(DataSourceBase.MyServices services, IContextResolverUserPermissions userPermissions) : RelationshipDataSourceBase(services, userPermissions, $"{LogPrefix}.Parent")
{
    /// <summary>
    /// Name of the field (in the parent) pointing to the child.
    /// If left blank, will use get all children.
    ///
    /// Example: If a person is referenced by books as both `Author` and `Illustrator` then leaving this empty will get both relationships, but specifying `Author` will only get this person if it's the author. 
    /// </summary>
    public override string FieldName => Configuration.GetThis();

    /// <summary>
    /// Name of the content-type to get.
    /// Will only get parents of the specified type.
    ///
    /// Example: If a person is referenced by books (as author) as by companies) as employee, then you may want to only find companies referencing this book. 
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
        => o => o.Relationships.FindParents(typeName, fieldName, Log);

}