using ToSic.Lib.Coding;

namespace ToSic.Eav.Apps;

public static class AppReaderFactoryTestAccessors
{
    public static IAppReader GetSystemPresetTac(this IAppReaderFactory factory, NoParamOrder protector = default, bool nullIfNotLoaded = false)
            => factory.GetSystemPreset(protector, nullIfNotLoaded);

    public static IAppReader GetTac(this IAppReaderFactory factory, int appId)
        => factory.Get(appId);


    public static IAppReader GetTac(this IAppReaderFactory factory, IAppIdentity app)
        => factory.Get(app);
}