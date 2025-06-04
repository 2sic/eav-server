using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.Sys.Insights.Data;

internal class InsightsTypeMetadata(LazySvc<IAppReaderFactory> appReaders) : InsightsProvider(Link, helpCategory: HiddenFromAutoDisplay, connect: [appReaders])
{
    public static string Link = "TypeMetadata";

    public override string Title => "Type Metadata";

    public override string HtmlBody()
    {
        var l = Log.Fn<string>($"appId:{AppId}; Type: {Type}");

        if (UrlParamsIncomplete(AppId, Type, out var message))
            return message;

        var typ = appReaders.Value.Get(AppId.Value).GetContentType(Type);

        var msg = H1($"Metadata for {typ.Name} ({typ.NameId}) in {AppId}\n").ToString();
        var metadata = typ.Metadata.ToList();

        return l.ReturnAsOk(MetadataHelper.MetadataTable(msg, AppId.Value, metadata, Linker));
    }

}