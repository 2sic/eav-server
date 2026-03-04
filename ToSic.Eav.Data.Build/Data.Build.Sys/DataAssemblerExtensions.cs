using System.Collections.Immutable;
using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Data.Build.Sys;

public static class DataAssemblerExtensions
{
    public static IImmutableList<IValue> ToValueList(this IValue value)
        => new List<IValue> { value }.ToImmutableOpt();

    public static IEntity FakeEntity(this DataAssembler dataAssembler, ContentTypeTypeAssembler typeAssembler, int appId)
        => dataAssembler.Entity.Create(
            appId: appId,
            attributes: dataAssembler.AttributeList.Finalize(new Dictionary<string, object?> { { AttributeNames.TitleNiceName, "" } }),
            contentType: typeAssembler.Transient("FakeEntity"),
            titleField: AttributeNames.TitleNiceName
        );

}
