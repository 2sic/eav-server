using ToSic.Eav.Context;

namespace ToSic.Eav.Ui.Edit.Sys.Load.Settings;

[ShowApiWhenReleased(ShowApiMode.Never)]
public record LoadSettingsProviderParameters
{
    public required IContextOfApp ContextOfApp { get; init; }

    public  required List<IContentType> ContentTypes { get; init; }

    public required List<string> InputTypes { get; init; }
}