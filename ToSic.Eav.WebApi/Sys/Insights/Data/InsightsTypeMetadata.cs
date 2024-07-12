using ToSic.Eav.Apps.Internal.Insights;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights;

internal class InsightsTypeMetadata(LazySvc<IAppStates> appStates) : InsightsProvider(Link, helpCategory: HiddenFromAutoDisplay, connect: [appStates])
{
    public static string Link = "TypeMetadata";

    public override string Title => "Type Metadata";

    public override string HtmlBody()
    {
        var l = Log.Fn<string>($"appId:{AppId}; Type: {Type}");

        if (UrlParamsIncomplete(AppId, Type, out var message))
            return message;

        var typ = appStates.Value.GetReader(AppId.Value).GetContentType(Type);

        var msg = H1($"Metadata for {typ.Name} ({typ.NameId}) in {AppId}\n").ToString();
        var metadata = typ.Metadata.ToList();

        return l.ReturnAsOk(MetadataHelper.MetadataTable(msg, AppId.Value, metadata, Linker));
    }

}