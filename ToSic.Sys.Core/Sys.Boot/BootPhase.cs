namespace ToSic.Sys.Boot;
public enum BootPhase
{
    Unknown = 0,
    Registrations = 1, // 1. Register services in DI container
    Initializations = 2, // 2. Initialize services (e.g. set up logging, etc.)
    Configurations = 3, // 3. Configure services (e.g. set up options, etc.)
    WarmUp = 4, // 4. Warm up services (e.g. pre-load data, etc.)
    Loading = 5,
    Finalize = 9, // 4. Finalize services (e.g. complete setup, etc.)
    Completed = 10 // 5. All boot phases completed

}
