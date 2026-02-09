using ToSic.Eav.Apps.Sys;

namespace ToSic.Eav.DataSource;

partial class DataSourceBase
{
    /// <inheritdoc />
    public IDataSourceConfiguration Configuration => field
        ??= ((DataSourceConfiguration)Services.Configuration).Attach(this);

    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public void Setup(IDataSourceOptions? options, IDataSourceLinkable? attach)
    {
        var l = Log.Fn();
        var attachLink = attach?.GetLink();
        var mainUpstream = attachLink?.DataSource;

        var appIdRequired = options?.AppIdentityOrReader
                            ?? mainUpstream
                            ?? throw new NullReferenceException("Setup needs a proper App ID, neither the options nor the attach.Link has it.");
        (this as IAppIdentitySync).UpdateAppIdentity(appIdRequired);
            
        // Attach in-bound, and make it immutable afterward
        if (attachLink == null)
            l.A("Nothing to attach");
        else
            Connect(attachLink);

        if (options?.Immutable == true)
            Immutable = true;
        l.A($"{nameof(Immutable)}, {Immutable}");

        var lookUp = options?.LookUp ?? mainUpstream?.Configuration?.LookUpEngine;
        if (lookUp != null && Configuration is DataSourceConfiguration dsConfig)
        {
            l.A("Add lookups");
            dsConfig.LookUpEngine = lookUp;
            var configValues = options?.MyConfigValues;
            if (configValues != null)
                dsConfig.AddMany(configValues.ToEditableIgnoreCase());
        }
        l.Done();
    }

}