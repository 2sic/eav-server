using ToSic.Eav.LookUp;

namespace ToSic.Eav.Apps;

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