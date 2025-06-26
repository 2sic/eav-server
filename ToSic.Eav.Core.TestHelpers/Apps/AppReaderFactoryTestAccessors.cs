using ToSic.Eav.Data;

namespace ToSic.Eav.Apps;

public static class AppReaderFactoryTestAccessors
{
    //public static IAppReader? GetSystemPresetTac(this IAppReaderFactory factory, NoParamOrder protector = default, bool nullIfNotLoaded = false)
    //        => factory.GetSystemPreset(protector, nullIfNotLoaded);

    public static IAppReader GetSystemPresetTac(this IAppReaderFactory factory)
            => factory.GetSystemPreset();

    public static IAppReader GetTac(this IAppReaderFactory factory, int appId)
        => factory.Get(appId);


    public static IAppReader GetTac(this IAppReaderFactory factory, IAppIdentity app)
        => factory.Get(app);

    public static IContentType GetContentTypeTac(this IAppReader reader, string nameId)
        => reader.GetContentType(nameId);
    public static IContentType? TryGetContentTypeTac(this IAppReader reader, string nameId)
        => reader.TryGetContentType(nameId);
}