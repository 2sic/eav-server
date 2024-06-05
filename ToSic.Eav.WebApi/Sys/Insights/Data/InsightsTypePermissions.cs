using ToSic.Eav.Apps.Internal.Insights;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights;

internal class InsightsTypePermissions(LazySvc<IAppStates> appStates) : InsightsProvider(Link, helpCategory: HiddenFromAutoDisplay, connect: [appStates])
{
    public static string Link = "TypePermissions";

    public override string Title => "Type Permissions";

    public override string HtmlBody()
    {
        var l = Log.Fn<string>($"appId:{AppId}; Type: {Type}");

        if (UrlParamsIncomplete(AppId, Type, out var message))
            return message;

        var typ = appStates.Value.GetReader(AppId.Value).GetContentType(Type);

        var msg = H1($"Permissions for {typ.Name} ({typ.NameId}) in {AppId}\n").ToString();
        var metadata = typ.Metadata.Permissions.Select(p => p.Entity).ToList();

        return l.ReturnAsOk(MetadataHelper.MetadataTable(msg, AppId.Value, metadata, Linker));
    }

}