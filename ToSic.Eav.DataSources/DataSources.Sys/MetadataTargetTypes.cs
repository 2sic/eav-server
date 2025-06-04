using System.Collections.Immutable;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Internal;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.DataSources.Sys;

/// <summary>
/// Get Metadata Target Types
/// </summary>
/// <remarks>
/// Added in v12.10
/// </remarks>
[VisualQuery(
    NiceName = "Metadata Target Types",
    UiHint = "Get Target Types which determine what kind of thing/target the metadata is for.",
    Icon = DataSourceIcons.MetadataTargetTypes,
    Type = DataSourceType.System,
    NameId = "fba0d40d-f6af-4593-9ccb-54cfd73d8217", // new generated
    Audience = Audience.Advanced,
    DynamicOut = false
)]
[InternalApi_DoNotUse_MayChangeWithoutNotice("WIP")]

public class MetadataTargetTypes : CustomDataSourceAdvanced
{
    public MetadataTargetTypes(MyServices services): base(services, $"{DataSourceConstantsInternal.LogPrefix}.MetaTg")
    {
        ProvideOut(GetList);
    }

    private IImmutableList<IEntity> GetList()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();

        var publicTargetTypes = Enum
            .GetValues(typeof(TargetTypes))
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
            // Sort, but ensure all the "Custom" are at the end
            .OrderBy(s => (s.Title.StartsWith("Custom") ? "Z" : "") + s.Title)
            .ToList();

        var dataFactory = DataFactory.SpawnNew(options: new()
        {
            AppId = 0,
            TitleField = AttributeNames.TitleNiceName,
            TypeName = "MetadataTargetTypes",
        });


        var list = publicTargetTypes
            .Select(set => dataFactory.Create(
                    new Dictionary<string, object>
                    {
                        { AttributeNames.TitleNiceName, set.Title },
                        { AttributeNames.NameIdNiceName, set.TargetType.ToString() }
                    },
                    id: (int)set.TargetType
                )
            )
            .ToImmutableList();
            
        return l.Return(list, $"{list.Count} items");
    }
}