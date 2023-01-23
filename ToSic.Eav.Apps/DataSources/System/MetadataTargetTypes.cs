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
        NiceName = "beta Metadata Target Types",
        UiHint = "Get Metadata target types",
        Icon = Icons.Metadata, // TODO
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
            var list = Enum.GetValues(typeof(TargetTypes)).Cast<TargetTypes>()
                .Select(value => DataBuilder.Entity(
                    new Dictionary<string, object>
                    {
                        { "Id", (int)value }, 
                        { "Name", value },
                        { "IsPrivate", Attribute.IsDefined(typeof(TargetTypes).GetField(value.ToString()), typeof(PrivateApi))}
                    },
                    appId: 0,
                    id: (int)value,
                    titleField: "Name",
                    typeName: "MetadataTargetTypes")
                ).ToImmutableArray();
            
            return (list, $"{list.Length} items");
        });
    }
}
