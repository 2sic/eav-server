using System;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Use this to take imported data from elsewhere which is a table but would have a tree-like structure (folders, etc.).
    /// Tell it where/how the relationships are mapped, and it will create Entities that have navigable relationships for this.
    /// </summary>
    /// <remarks>
    /// New in v11.20
    /// </remarks>
    [VisualQuery(
        GlobalName = "58cfcbd6-e2ae-40f7-9acf-ac8d758adff9",
        NiceName = "Relationship/Tree Modeler",
        UiHint = "Connect items to create relationships or trees",
        Icon = Icons.Tree,
        PreviousNames = new[]
        {
            "58cfcbd6-e2ae-40f7-9acf-ac8d758adff9",
            "ToSic.Eav.DataSources.TreeBuilder, ToSic.Eav.DataSources.SharePoint"
        },
        Type = DataSourceType.Modify,
        ExpectsDataOfType = "d167054a-fe0f-4e98-b1f1-0a9990873e86",
        In = new[] { Constants.DefaultStreamName + "*" },
        HelpLink = "https://r.2sxc.org/DsTreeModeler")]
    [PublicApi("Brand new in v11.20, WIP, may still change a bit")]
    // ReSharper disable once UnusedMember.Global
    public sealed class TreeModeler : DataSource
    {
        private readonly ITreeMapper _treeMapper;

        #region Constants & Properties

        /// <summary>
        /// This determines what property is used as ID on the parent.
        /// Currently only allows "EntityId" and "EntityGuid"
        /// </summary>
        public string Identifier
        {
            get => Configuration.GetThis();
            set => Configuration.SetThis(value);
        }

        /// <summary>
        /// The property on a child which contains the parent ID
        /// </summary>
        public string ParentReferenceField
        {
            get => Configuration.GetThis();
            set => Configuration.SetThis(value);
        }

        /// <summary>
        /// The name of the new field on the parent, which will reference the children
        /// </summary>
        public string NewChildrenField
        {
            get => Configuration.GetThis();
            set => Configuration.SetThis(value);
        }

        /// <summary>
        /// Name of the new field on a child, which will reference the parent. 
        /// </summary>
        public string NewParentField
        {
            get => Configuration.GetThis();
            set => Configuration.SetThis(value);
        }

        #endregion

        /// <summary>
        /// Initializes this data source
        /// </summary>
        [PrivateApi]
        public TreeModeler(Dependencies dependencies, ITreeMapper treeMapper) : base(dependencies, $"{DataSourceConstants.LogPrefix}.Tree")
        {
            ConnectServices(
                _treeMapper = treeMapper
            );
            // Specify what out-streams this data-source provides. Usually just one, called "Default"
            Provide(GetList);

            ConfigMaskMyConfig(nameof(Identifier), "ParentIdentifierAttribute||EntityId");
            ConfigMaskMyConfig(nameof(ParentReferenceField), "ChildParentAttribute||ParentId");
            ConfigMaskMyConfig(nameof(NewChildrenField), "TargetChildrenAttribute||Children");
            ConfigMaskMyConfig(nameof(NewParentField), "TargetParentAttribute||Parent");
        }

        /// <summary>
        /// Internal helper that returns the entities
        /// </summary>
        /// <returns></returns>
        private IImmutableList<IEntity> GetList() => Log.Func(() =>
        {
            Configuration.Parse();

            if (!GetRequiredInList(out var originals))
                return (originals, "error");

            switch (Identifier)
            {
                case "EntityGuid":
                    var resultGuid = _treeMapper.AddRelationships<Guid>(
                        originals, Identifier, ParentReferenceField,
                        NewChildrenField, NewParentField);
                    return (resultGuid, $"Guid: {resultGuid.Count}");
                case "EntityId":
                    var resultInt = _treeMapper.AddRelationships<int>(
                        originals, Identifier, ParentReferenceField,
                        NewChildrenField, NewParentField);
                    return (resultInt, $"int: {resultInt.Count}");
                default:
                    return (SetError("Invalid Identifier", "TreeBuilder currently supports EntityGuid or EntityId as parent identifier attribute."), "error");
            }
        });

    }
}
