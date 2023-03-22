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
        NameId = "58cfcbd6-e2ae-40f7-9acf-ac8d758adff9",
        NiceName = "Relationship/Tree Modeler",
        UiHint = "Connect items to create relationships or trees",
        Icon = Icons.Tree,
        NameIds = new[]
        {
            "58cfcbd6-e2ae-40f7-9acf-ac8d758adff9",
            "ToSic.Eav.DataSources.TreeBuilder, ToSic.Eav.DataSources.SharePoint"
        },
        Type = DataSourceType.Modify,
        ConfigurationType = "d167054a-fe0f-4e98-b1f1-0a9990873e86",
        In = new[] { DataSourceConstants.StreamDefaultName + "*" },
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
        [Configuration(Field = "ParentIdentifierAttribute", Fallback = Attributes.EntityFieldId)]
        public string Identifier
        {
            get => Configuration.GetThis();
            set => Configuration.SetThis(value);
        }

        /// <summary>
        /// The property on a child which contains the parent ID
        /// </summary>
        [Configuration(Field = "ChildParentAttribute", Fallback = "ParentId")]
        public string ParentReferenceField
        {
            get => Configuration.GetThis();
            set => Configuration.SetThis(value);
        }

        /// <summary>
        /// The name of the new field on the parent, which will reference the children
        /// </summary>
        [Configuration(Field = "TargetChildrenAttribute", Fallback = "Children")]
        public string NewChildrenField
        {
            get => Configuration.GetThis();
            set => Configuration.SetThis(value);
        }

        /// <summary>
        /// Name of the new field on a child, which will reference the parent. 
        /// </summary>
        [Configuration(Field = "TargetParentAttribute", Fallback = "Parent")]
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
        public TreeModeler(MyServices services, ITreeMapper treeMapper) : base(services, $"{DataSourceConstants.LogPrefix}.Tree")
        {
            ConnectServices(
                _treeMapper = treeMapper
            );
            // Specify what out-streams this data-source provides. Usually just one, called "Default"
            ProvideOut(GetList);
        }

        /// <summary>
        /// Internal helper that returns the entities
        /// </summary>
        /// <returns></returns>
        private IImmutableList<IEntity> GetList() => Log.Func(() =>
        {
            Configuration.Parse();

            var source = TryGetIn();
            if (source is null) return (Error.TryGetInFailed(), "error");

            var tm = (TreeMapper)_treeMapper;
            switch (Identifier)
            {
                case "EntityGuid":
                    var resultGuid = tm.AddParentChild(
                        source, Identifier, ParentReferenceField,
                        NewChildrenField, NewParentField);
                    return (resultGuid, $"Guid: {resultGuid.Count}");
                case "EntityId":
                    var resultInt = tm.AddParentChild(
                        source, Identifier, ParentReferenceField,
                        NewChildrenField, NewParentField);
                    return (resultInt, $"int: {resultInt.Count}");
                default:
                    return (Error.Create(
                            title: "Invalid Identifier",
                            message:
                            "TreeBuilder only supports EntityGuid or EntityId as parent identifier attribute."),
                        "error");
            }
        });

    }
}
