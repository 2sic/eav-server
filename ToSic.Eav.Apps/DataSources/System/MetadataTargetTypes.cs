using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
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
        Audience = Audience.Advanced,
        DynamicOut = false
    )]
    [InternalApi_DoNotUse_MayChangeWithoutNotice("WIP")]

    public class MetadataTargetTypes : DataSource
    {
        private readonly IDataFactory _dataFactory;

        public MetadataTargetTypes(MyServices services, IDataFactory dataFactory) : base(services, $"{DataSourceConstants.LogPrefix}.MetaTg")
        {
            ConnectServices(
                _dataFactory = dataFactory.New(appId: 0, typeName: "MetadataTargetTypes", titleField: Data.Attributes.TitleNiceName)
            );
            Provide(GetList);
        }

        private IImmutableList<IEntity> GetList() => Log.Func(l =>
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
                // Sort, but ensure all the "Custom" are at the end
                .OrderBy(s => (s.Title.StartsWith("Custom") ? "Z" : "") + s.Title)
                .ToList();

            var list = publicTargetTypes
                .Select(set => _dataFactory.Create(
                    new Dictionary<string, object>
                    {
                        { Data.Attributes.TitleNiceName, set.Title },
                        { Data.Attributes.NameIdNiceName, set.TargetType.ToString() }
                    },
                    id: (int)set.TargetType
                    )
                ).ToImmutableList();
            
            return (list, $"{list.Count} items");
        });
    }
}
