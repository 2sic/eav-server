using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.Build;

public static class DataFactoryTestAccessors
{
    public static IEntity CreateTac(this IDataFactory factory, IConvertibleToRawEntity item) =>
        factory.Create(item);
}
