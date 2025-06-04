using ToSic.Sys.Boot;

namespace ToSic.Eav.Sys.Capabilities.Features;

[PrivateApi]
public class EavBootLoadFeaturesAndLicenses(ILogStore logStore, EavFeaturesLoader featuresLoader)
    : BootProcessBase("EavLnF",
        bootPhase: BootPhase.Loading,
        priority: 1000, // Depends on the preset app being loaded first, for the content-types
        connect: [logStore, featuresLoader]
    )
{
    private static bool _startupAlreadyRan;

    /// <summary>
    /// Do things we need at application start
    /// </summary>
    public override void Run()
    {
        var l = BootLog.Log.Fn("Eav: StartUp", timer: true);
        // Prevent multiple Initializations
        if (_startupAlreadyRan)
            throw new("Startup should never be called twice.");
        _startupAlreadyRan = true;


        // Finally, load all the licenses and features
        featuresLoader.LoadLicenseAndFeatures();

        l.Done("startup complete");
    }

}