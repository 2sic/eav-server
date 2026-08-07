using System.Collections.Immutable;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Entities;

namespace ToSic.Eav.Data.Build.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class DataAssemblerExtensions
{
    /// <summary>
    /// This is the content-type name of fake entities.
    /// It could change at any time, but it should be a bit complex so it's not likely to be used elsewhere.
    /// This is also used by the toolbar to filter out this type.
    /// </summary>
    public const string FakeEntityContentType = "FakeEntityType";
    
    public static IImmutableList<IValue> ToValueList(this IValue value)
        => new List<IValue> { value }.ToImmutableOpt();

    public static IEntity FakeEntity(this DataAssembler dataAssembler, ContentTypeAssembler typeAssembler, int appId)
        => dataAssembler.Entity.Create(
            appId: appId,
            attributes: dataAssembler.AttributeList.Finalize(new Dictionary<string, object?> { { AttributeNames.TitleNiceName, "" } }),
            contentType: typeAssembler.Transient(FakeEntityContentType),
            titleField: AttributeNames.TitleNiceName
        );

    /// <summary>
    /// Create an empty entity of a specific type.
    /// Usually used in edit scenarios, where the presentation doesn't exist yet
    /// </summary>
    public static IEntity EmptyOfType(this DataAssembler dataAssembler, int appId, Guid entityGuid, int entityId, IContentType type) =>
        dataAssembler.Entity.Create(appId: appId,
            entityId: entityId,
            guid: entityGuid,
            contentType: type,
            attributes: dataAssembler.AttributeList
                .CreateListForType(type, null),
            created: DateTime.MinValue,
            modified: DateTime.Now,
            owner: ""
        );

}
