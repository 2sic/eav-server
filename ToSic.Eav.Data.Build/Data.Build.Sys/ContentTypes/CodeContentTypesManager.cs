namespace ToSic.Eav.Data.Build.Sys;

/// <summary>
/// Special system to manage and to convert c# classes with their definitions/attributes into content types.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[method: PrivateApi]
public class CodeContentTypesManager(LazySvc<CodeContentTypeBuilder> ctBuilder)
    : ServiceBase("Eav.CtFact")
{
    // TODO: Should probably be something different...?
    public const int NoAppId = -1;

    public IContentType Get<T>()
        => Get(typeof(T));

    public IContentType Get(Type type)
    {
        if (Cache.TryGetValue(type, out var contentType))
            return contentType;
        var created = ctBuilder.Value.Generate(type,  name: null, nameId: null, scope: null);
        Cache[type] = created;
        return created;
    }

    private static readonly Dictionary<Type, IContentType> Cache = new();

}