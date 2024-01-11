using ToSic.Eav.LookUp;

namespace ToSic.Eav.Apps;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AppDataConfiguration(ILookUpEngine configuration, bool? showDrafts = null) : IAppDataConfiguration
{
    public bool? ShowDrafts { get; } = showDrafts;


    public ILookUpEngine Configuration { get; } = configuration;
}