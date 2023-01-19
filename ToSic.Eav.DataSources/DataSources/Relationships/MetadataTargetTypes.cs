using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.DataSources.Sys.Types;
using ToSic.Eav.Metadata;
using ToSic.Lib.Documentation;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources.Sys
{
    /// <summary>
    /// Get Target Types (metadata targets) of the Entities coming into this DataSource
    /// </summary>
    /// <remarks>
    /// Added in v12.10
    /// </remarks>
    [VisualQuery(
        NiceName = "beta Metadata Target Types",
        UiHint = "Get the item's target types (if they are metadata)",
        Icon = Icons.Metadata, // TODO
        Type = DataSourceType.Lookup,
        GlobalName = "fba0d40d-f6af-4593-9ccb-54cfd73d8217", // new generated
        In = new[] { Constants.DefaultStreamNameRequired },
        DynamicOut = false,
        ExpectsDataOfType = "7dcd26eb-a70c-4a4f-bb3b-5bd5da304232", // TODO - this is the wrong type
        Difficulty = DifficultyBeta.Advanced)]
    [InternalApi_DoNotUse_MayChangeWithoutNotice("WIP")]

    public class MetadataTargetTypes : MetadataDataSourceBase
    {
        /// <summary>
        /// TODO
        /// </summary>
        public override string ContentTypeName
        {
            get => Configuration[nameof(ContentTypeName)];
            set => Configuration[nameof(ContentTypeName)] = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Defaults to true
        /// </remarks>
        public bool FilterDuplicates
        {
            get => Configuration.GetBool(nameof(FilterDuplicates));
            set => Configuration.SetBool(nameof(FilterDuplicates), value);
        }
        public MetadataTargetTypes(IAppStates appStates, Dependencies dependencies) : base(dependencies, $"{DataSourceConstants.LogPrefix}.MetaTg")
        {
            ConfigMask(nameof(FilterDuplicates) + "||true");
            _appStates = appStates;
        }

        protected override IEnumerable<IEntity> SpecificGet(IImmutableList<IEntity> originals, string typeName)
        {
            var find = InnerGet();

            var relationships = originals.SelectMany(o => find(o));

            if (FilterDuplicates) relationships = relationships.Distinct(/*new EqualityComparerIEntity()*/); // TODO: this is not working

            if (!string.IsNullOrWhiteSpace(typeName))
                relationships = relationships.OfType(typeName);
            
            return relationships;
        }

        /// <summary>
        /// Construct function for the get of the related items
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        [PrivateApi]
        private Func<IEntity, IEnumerable<IEntity>> InnerGet()
        {
            return o =>
            {
                var mdFor = o.MetadataFor;

                if (!mdFor.IsMetadata || mdFor.TargetType != (int)TargetTypes.ContentType) return Enumerable.Empty<IEntity>();

                IContentType t = null;
                if (mdFor.KeyString != null) t = AppState.ContentTypes.FirstOrDefault(c => c.Is(mdFor.KeyString));
                if (t == null) return Enumerable.Empty<IEntity>();
                Guid? guid = null;
                try
                {
                    if (Guid.TryParse(t.NameId, out var g)) guid = g;
                }
                catch
                {
                    /* ignore */
                }
                var e = DataBuilder.Entity(ContentTypeUtil.BuildDictionary(t),
                    appId: AppState.AppId,
                    id: t.Id,
                    titleField: ContentTypeType.Name.ToString(),
                    typeName: t.Name,
                    guid: guid);
                return (new[] { e });
            };
        }

        private AppState AppState => _appState ?? (_appState = _appStates.Get(this));
        public AppState _appState;
        private readonly IAppStates _appStates;
    }

    //public class EqualityComparerIEntity : IEqualityComparer<IEntity>
    //{
    //    public bool Equals(IEntity x, IEntity y)
    //        => x != null && x.Name == y?.Name;
        
    //    public int GetHashCode(IEntity obj)
    //        => obj.Title.GetHashCode();
    //}
}
