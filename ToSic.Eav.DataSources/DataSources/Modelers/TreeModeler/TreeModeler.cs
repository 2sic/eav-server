using System;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
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
        #region Constants & Properties

        private const string ParentIdentifierAttributeConfigKey = "ParentIdentifierAttribute";
        private const string ChildParentAttributeConfigKey = "ChildParentAttribute";
        private const string TargetChildrenAttributeConfigKey = "TargetChildrenAttribute";
        private const string TargetParentAttributeConfigKey = "TargetParentAttribute";


        /// <summary>
        /// This determines what property is used as ID on the parent.
        /// Currently only allows "EntityId" and "EntityGuid"
        /// </summary>
        public string Identifier
        {
            get => Configuration[ParentIdentifierAttributeConfigKey];
            set => Configuration[ParentIdentifierAttributeConfigKey] = value;
        }

        /// <summary>
        /// The property on a child which contains the parent ID
        /// </summary>
        public string ParentReferenceField
        {
            get => Configuration[ChildParentAttributeConfigKey];
            set => Configuration[ChildParentAttributeConfigKey] = value;
        }

        /// <summary>
        /// The name of the new field on the parent, which will reference the children
        /// </summary>
        public string NewChildrenField
        {
            get => Configuration[TargetChildrenAttributeConfigKey];
            set => Configuration[TargetChildrenAttributeConfigKey] = value;
        }

        /// <summary>
        /// Name of the new field on a child, which will reference the parent. 
        /// </summary>
        public string NewParentField
        {
            get => Configuration[TargetParentAttributeConfigKey];
            set => Configuration[TargetParentAttributeConfigKey] = value;
        }

        #endregion

        /// <summary>
        /// Initializes this data source
        /// </summary>
        [PrivateApi]
        public TreeModeler(MultiBuilder multiBuilder, Dependencies dependencies) : base(dependencies, $"{DataSourceConstants.LogPrefix}.Tree")
        {
            ConnectServices(
                _multiBuilder = multiBuilder
            );
            // Specify what out-streams this data-source provides. Usually just one, called "Default"
            Provide(GetList);

            ConfigMask(ParentIdentifierAttributeConfigKey);
            ConfigMask(ChildParentAttributeConfigKey);
            ConfigMask(TargetChildrenAttributeConfigKey);
            ConfigMask(TargetParentAttributeConfigKey);
        }

        private readonly MultiBuilder _multiBuilder;

        /// <summary>
        /// Internal helper that returns the entities
        /// </summary>
        /// <returns></returns>
        private IImmutableList<IEntity> GetList() => Log.Func(() =>
        {
            Configuration.Parse();

            if (!GetRequiredInList(out var originals))
                return (originals, "error");

            ITreeMapper treeMapper;
            switch (Identifier)
            {
                case "EntityGuid":
                    treeMapper = new TreeMapper<Guid>(_multiBuilder, Log);
                    break;
                case "EntityId":
                    treeMapper = new TreeMapper<int>(_multiBuilder, Log);
                    break;
                default:
                    return (SetError("Invalid Identifier", "TreeBuilder currently supports EntityGuid or EntityId as parent identifier attribute."), "error");
            }
            var res = treeMapper.GetEntitiesWithRelationships(originals, Identifier, ParentReferenceField, NewChildrenField, NewParentField);
            return (res, $"{res.Count}");
        });

    }
}
