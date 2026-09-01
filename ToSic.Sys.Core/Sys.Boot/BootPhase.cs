namespace ToSic.Sys.Boot;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public enum BootPhase
{
    Unknown = 0,
    
    /// <summary>
    /// 1. Register services in DI container
    /// </summary>
    Registrations = 1,
    
    /// <summary>
    /// 2. Initialize services (e.g. set up logging, etc.)
    /// </summary>
    Initializations = 2,
    
    /// <summary>
    /// 3. Configure services (e.g. set up options, etc.)
    /// </summary>
    Configurations = 3,
    
    /// <summary>
    /// 4. Warm up services (e.g. pre-load data, etc.)
    /// </summary>
    WarmUp = 4,
    
    /// <summary>
    /// 5. Loading
    /// </summary>
    Loading = 5,
    
    /// <summary>
    /// 9. Finalize services (e.g. complete setup, etc.)
    /// </summary>
    Finalize = 9,
    
    /// <summary>
    /// 10. All boot phases completed
    /// </summary>
    Completed = 10

}
