using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Metadata;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources.Sys
{
    /// <summary>
    /// Get Metadata Target Types
    /// </summary>
    /// <remarks>
    /// Added in v12.10
    /// </remarks>
    [VisualQuery(
        NiceName = "Metadata Target Types",
        UiHint = "Get Target Types which determine what kind of thing/target the metadata is for.",
        Icon = Icons.MetadataTargetTypes,
        Type = DataSourceType.System,
        GlobalName = "fba0d40d-f6af-4593-9ccb-54cfd73d8217", // new generated
        Difficulty = DifficultyBeta.Advanced,
        DynamicOut = false
    )]
    [InternalApi_DoNotUse_MayChangeWithoutNotice("WIP")]

    public class MetadataTargetTypes : DataSource
    {
        public MetadataTargetTypes(Dependencies dependencies) : base(dependencies, $"{DataSourceConstants.LogPrefix}.MetaTg")
        {
            Provide(GetList);
        }

        private ImmutableArray<IEntity> GetList() => Log.Func(l =>
        {
            var publicTargetTypes = Enum.GetValues(typeof(TargetTypes))
                .Cast<TargetTypes>()
                .Select(value =>
                {
                    var field = typeof(TargetTypes).GetField(value.ToString());
                    return new
                    {
                        TargetType = value,
                        IsPrivate = Attribute.IsDefined(field, typeof(PrivateApi)),
                        Docs = Attribute.GetCustomAttribute(field, typeof(DocsWip)) as DocsWip
                    };
                })
                .Where(value => !value.IsPrivate)
                .Select(value => new
                {
                    value.TargetType,
                    Title = value.Docs?.Documentation ?? value.TargetType.ToString()
                })
                .OrderBy(s => s.Title)
                .ToList();

            // TODO: @2dm "Title" should be a global constant
            // TODO: @2dm "NameId" should be a global constant

            var list = publicTargetTypes
                .Select(set => DataBuilder.Entity(
                    new Dictionary<string, object>
                    {
                        { "Id", (int)set.TargetType }, 
                        { DataConstants.TitleField, set.Title },
                        { DataConstants.NameIdField, set.TargetType.ToString() }
                    },
                    appId: 0,
                    id: (int)set.TargetType,
                    titleField: DataConstants.TitleField,
                    typeName: "MetadataTargetTypes")
                ).ToImmutableArray();
            
            return (list, $"{list.Length} items");
        });
    }
}
