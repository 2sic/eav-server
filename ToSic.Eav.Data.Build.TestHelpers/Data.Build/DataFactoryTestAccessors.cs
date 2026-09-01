using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.Data.Build;

public static class DataFactoryTestAccessors
{
    public static IEntity CreateTac(this IDataFactory factory, IRawData item) =>
        factory.Create(item);
}
