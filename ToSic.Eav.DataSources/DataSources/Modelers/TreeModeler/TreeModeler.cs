﻿using ToSic.Eav.Data.Sys;
using ToSic.Eav.DataSource.Sys;
using ToSic.Eav.DataSources.Sys;

namespace ToSic.Eav.DataSources;

/// <summary>
/// Use this to take imported data from elsewhere which is a table but would have a tree-like structure (folders, etc.).
/// Tell it where/how the relationships are mapped, and it will create Entities that have navigable relationships for this.
/// </summary>
/// <remarks>
/// * New in v11.20
/// * Changed in v15.05 to use the [immutable convention](xref:NetCode.Conventions.Immutable)
/// * note that the above change is actually a breaking change, but since this is such an advanced DataSource, we assume it's not used in dynamic code.
/// </remarks>
[VisualQuery(
    NameId = "58cfcbd6-e2ae-40f7-9acf-ac8d758adff9",
    NiceName = "Relationship/Tree Modeler",
    UiHint = "Connect items to create relationships or trees",
    Icon = DataSourceIcons.Tree,
    NameIds =
    [
        "58cfcbd6-e2ae-40f7-9acf-ac8d758adff9",
        "ToSic.Eav.DataSources.TreeBuilder, ToSic.Eav.DataSources.SharePoint"
    ],
    Type = DataSourceType.Modify,
    ConfigurationType = "d167054a-fe0f-4e98-b1f1-0a9990873e86",
    In = [DataSourceConstants.StreamDefaultName + "*"],
    HelpLink = "https://go.2sxc.org/DsTreeModeler")]
[PublicApi("Brand new in v11.20, WIP, may still change a bit")]
// ReSharper disable once UnusedMember.Global
public sealed class TreeModeler : DataSourceBase
{
    private readonly ITreeMapper _treeMapper;

    #region Constants & Properties

    /// <summary>
    /// This determines what property is used as ID on the parent.
    /// Currently only allows "EntityId" and "EntityGuid"
    /// </summary>
    [Configuration(Field = "ParentIdentifierAttribute", Fallback = AttributeNames.EntityFieldId)]
    public string Identifier => Configuration.GetThis(fallback: AttributeNames.EntityFieldId);

    /// <summary>
    /// The property on a child which contains the parent ID
    /// </summary>
    [Configuration(Field = "ChildParentAttribute", Fallback = "ParentId")]
    public string ParentReferenceField => Configuration.GetThis(fallback: "ParentId");

    /// <summary>
    /// The name of the new field on the parent, which will reference the children
    /// </summary>
    [Configuration(Field = "TargetChildrenAttribute", Fallback = "Children")]
    public string NewChildrenField => Configuration.GetThis(fallback: "Children");

    /// <summary>
    /// Name of the new field on a child, which will reference the parent. 
    /// </summary>
    [Configuration(Field = "TargetParentAttribute", Fallback = "Parent")]
    public string NewParentField => Configuration.GetThis(fallback: "Parent");

    #endregion

    /// <summary>
    /// Initializes this data source
    /// </summary>
    [PrivateApi]
    public TreeModeler(Dependencies services, ITreeMapper treeMapper) : base(services, $"{DataSourceConstantsInternal.LogPrefix}.Tree", connect: [treeMapper])
    {
        _treeMapper = treeMapper;
        // Specify what out-streams this data-source provides. Usually just one, called "Default"
        ProvideOut(GetList);
    }

    /// <summary>
    /// Internal helper that returns the entities
    /// </summary>
    /// <returns></returns>
    private IImmutableList<IEntity> GetList()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        Configuration.Parse();

        var source = TryGetIn();
        if (source is null)
            return l.ReturnAsError(Error.TryGetInFailed());

        switch (Identifier)
        {
            case AttributeNames.EntityGuidPascalCase:
                var resultGuid = _treeMapper.AddParentChild(
                    originals: source,
                    parentIdField: Identifier,
                    childToParentRefField: ParentReferenceField,
                    newChildrenField: NewChildrenField,
                    newParentField: NewParentField);
                return l.Return(resultGuid, $"Guid: {resultGuid.Count}");
            case AttributeNames.EntityIdPascalCase:
                var resultInt = _treeMapper.AddParentChild(
                    originals: source,
                    parentIdField: Identifier,
                    childToParentRefField: ParentReferenceField,
                    newChildrenField: NewChildrenField,
                    newParentField: NewParentField);
                return l.Return(resultInt, $"int: {resultInt.Count}");
            default:
                return l.ReturnAsError(Error.Create(
                    title: "Invalid Identifier",
                    message:
                    "TreeBuilder only supports EntityGuid or EntityId as parent identifier attribute."));
        }
    }

}