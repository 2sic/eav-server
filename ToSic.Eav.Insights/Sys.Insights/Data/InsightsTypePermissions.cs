using ToSic.Eav.Apps.Internal.Insights;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights;

internal class InsightsTypePermissions(LazySvc<IAppReaderFactory> appReaders) : InsightsProvider(Link, helpCategory: HiddenFromAutoDisplay, connect: [appReaders])
{
    public static string Link = "TypePermissions";

    public override string Title => "Type Permissions";

    public override string HtmlBody()
    {
        var l = Log.Fn<string>($"appId:{AppId}; Type: {Type}");

        if (UrlParamsIncomplete(AppId, Type, out var message))
            return message;

        var typ = appReaders.Value.Get(AppId.Value).GetContentType(Type);

        var msg = H1($"Permissions for {typ.Name} ({typ.NameId}) in {AppId}\n").ToString();
        var metadata = typ.Metadata.Permissions
            .Select(p => ((ICanBeEntity)p).Entity)
            .ToList();

        return l.ReturnAsOk(MetadataHelper.MetadataTable(msg, AppId.Value, metadata, Linker));
    }

}