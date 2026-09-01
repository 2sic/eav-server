using ToSic.Sys.Utils.Types;

namespace ToSic.Sys.Utils.TypeFactoryTests;

#pragma warning disable CS9113
public class TypeFactoryTac
{
    public static object CreateInstanceTac(Type type)
        => TypeFactory.CreateInstance(type);

    public static T CreateInstanceTac<T>() where T : class
        => TypeFactory.CreateInstance<T>();
}