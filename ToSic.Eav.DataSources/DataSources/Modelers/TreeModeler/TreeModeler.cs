using System;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.DataSources.Queries;

namespace ToSic.Eav.DataSources
{
    // TODO 2021-04-07
    // More changes should be made, but we couldn't test it so we delayed this
    // 1. Should change to use AttributeBuilder with DI
    
    /// <inheritdoc />
    /// <summary>
    /// TODO
    /// </summary>
    [VisualQuery(
        GlobalName = "58cfcbd6-e2ae-40f7-9acf-ac8d758adff9",
        NiceName = "Relationship/Tree Modeler",
        UiHint = "Connect items to create relationships or trees",
        Icon = "account_tree",
        PreviousNames = new []
        {
            "58cfcbd6-e2ae-40f7-9acf-ac8d758adff9",
            "ToSic.Eav.DataSources.TreeBuilder, ToSic.Eav.DataSources.SharePoint"
        },
        Type = DataSourceType.Modify,
        ExpectsDataOfType = "d167054a-fe0f-4e98-b1f1-0a9990873e86",
        In = new [] { Constants.DefaultStreamName + "*"},
        HelpLink = "")] // ToDo: Helplink

    public sealed class RelationshipModeler : DataSourceBase
    {
        private readonly AttributeBuilder _attributeBuilderWip;

        #region Constants & Properties
        public override string LogId => "Ds.Tree"; // this text is added to all internal logs, so it's easier to debug

        private const string ParentIdentifierAttributeConfigKey = "ParentIdentifierAttribute";
        private const string ChildParentAttributeConfigKey = "ChildParentAttribute";
        private const string TargetChildrenAttributeConfigKey = "TargetChildrenAttribute";
        private const string TargetParentAttributeConfigKey = "TargetParentAttribute";

        private string ParentIdentifierAttribute
        {
            get => Configuration[ParentIdentifierAttributeConfigKey];
            set => Configuration[ParentIdentifierAttributeConfigKey] = value;
        }
        private string ChildParentAttribute
        {
            get => Configuration[ChildParentAttributeConfigKey];
            set => Configuration[ChildParentAttributeConfigKey] = value;
        }
        private string TargetChildrenAttribute
        {
            get => Configuration[TargetChildrenAttributeConfigKey];
            set => Configuration[TargetChildrenAttributeConfigKey] = value;
        }
        private string TargetParentAttribute
        {
            get => Configuration[TargetParentAttributeConfigKey];
            set => Configuration[TargetParentAttributeConfigKey] = value;
        }

        #endregion
        
        /// <summary>
        /// Initializes this data source
        /// </summary>
        public RelationshipModeler(AttributeBuilder attributeBuilderWip)
        {
            _attributeBuilderWip = attributeBuilderWip;
            // Specify what out-streams this data-source provides. Usually just one, called "Default"
            Provide(GetList);

            ConfigMask(ParentIdentifierAttributeConfigKey, $"[Settings:{ParentIdentifierAttributeConfigKey}]");
            ConfigMask(ChildParentAttributeConfigKey, $"[Settings:{ChildParentAttributeConfigKey}]");
            ConfigMask(TargetChildrenAttributeConfigKey, $"[Settings:{TargetChildrenAttributeConfigKey}]");
            ConfigMask(TargetParentAttributeConfigKey, $"[Settings:{TargetParentAttributeConfigKey}]");
        }

        /// <summary>
        /// Internal helper that returns the entities
        /// </summary>
        /// <returns></returns>
        private IImmutableList<IEntity> GetList()
        {
            var wrapLog = Log.Call<IImmutableList<IEntity>>();
            Configuration.Parse();

            if (!GetRequiredInList(out var originals))
                return wrapLog("error", originals);

            ITreeMapper treeMapper;
            switch (ParentIdentifierAttribute)
            {
                case "EntityGuid":
                    treeMapper = new TreeMapper<Guid>(_attributeBuilderWip);
                    break;
                case "EntityId":
                    treeMapper = new TreeMapper<int>(_attributeBuilderWip);
                    break;
                default:
                    return wrapLog("error", SetError("Invalid Identifier",
                        "TreeBuilder currently supports EntityGuid or EntityId as parent identifier attribute."));
            }
            var res  = treeMapper.GetEntitiesWithRelationships(originals, ParentIdentifierAttribute, ChildParentAttribute, TargetChildrenAttribute, TargetParentAttribute);
            return wrapLog($"{res.Count}", res);
        }

    }
}
