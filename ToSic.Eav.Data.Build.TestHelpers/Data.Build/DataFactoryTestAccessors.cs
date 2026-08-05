using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.Build;

public static class DataFactoryTestAccessors
{
    public static IEntity CreateTac(this IDataFactory factory, IRawEntitySource item) =>
        factory.Create(item);
}
