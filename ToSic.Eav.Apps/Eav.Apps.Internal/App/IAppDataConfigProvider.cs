namespace ToSic.Eav.Apps.Internal;

public interface IAppDataConfigProvider
{
    public IAppDataConfiguration GetDataConfiguration(EavApp app, AppDataConfigSpecs specs);

}