using ToSic.Eav.Internal.Unknown;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.Apps.Internal;

internal class AppDataConfigProviderUnknown(WarnUseOfUnknown<AppDataConfigProviderUnknown> _) : IAppDataConfigProvider
{
    public IAppDataConfiguration GetDataConfiguration(EavApp app, AppDataConfigSpecs specs) => new AppDataConfiguration(new LookUpEngine(app.Log));
}