namespace ToSic.Eav.Ui.Edit.Sys.Load.Settings;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ILoadSettingsProvider: IHasLog
{
    Dictionary<string, object> GetSettings(LoadSettingsProviderParameters parameters);
}