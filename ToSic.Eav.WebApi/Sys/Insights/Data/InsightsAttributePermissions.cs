using ToSic.Eav.Apps.Internal.Insights;
using ToSic.Eav.WebApi.Errors;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights;

internal class InsightsAttributePermissions(LazySvc<IAppReaderFactory> appReaders) : InsightsProvider(Link, helpCategory: HiddenFromAutoDisplay, connect: [appReaders])
{
    public static string Link = "AttributePermissions";

    public override string Title => "Attribute Permissions";

    public override string HtmlBody()
    {
        if (UrlParamsIncomplete(AppId, Type, NameId, out var message))
            return message;

        Log.A($"debug app metadata for {AppId} and {Type}");
        var typ = appReaders.Value.Get(AppId.Value).GetContentType(Type);
        var att = typ.Attributes.First(a => a.Name == NameId)
                  ?? throw HttpException.BadRequest($"can't find attribute {NameId}");

        var msg = H1($"Attribute Permissions for {typ.Name}.{NameId} in {AppId}\n").ToString();
        var metadata = att.Metadata.Permissions.Select(p => p.Entity).ToList();

        return MetadataHelper.MetadataTable(msg, AppId.Value, metadata, Linker);
    }


}