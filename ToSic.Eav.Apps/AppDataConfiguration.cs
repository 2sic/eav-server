using ToSic.Eav.LookUp;

namespace ToSic.Eav.Apps;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AppDataConfiguration: IAppDataConfiguration
{
    public AppDataConfiguration(ILookUpEngine configuration, bool? showDrafts = null)
    {
        ShowDrafts = showDrafts;
        Configuration = configuration;
    }

    public bool? ShowDrafts { get; }


    public ILookUpEngine Configuration { get; }

}