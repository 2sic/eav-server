﻿namespace ToSic.Eav.DataSources;

/// <summary>
/// Internal DataSource to generate an error on purpose.
/// This is to test / verify error handling in VisualQuery. See also [](xref:Basics.Query.Debug.Index)
/// </summary>
/// <remarks>
/// In advanced programming scenarios you can also use this DataSource instead of another one to provide a stream of errors.
/// </remarks>
[VisualQuery(
    NiceName = "Error DataSource",
    UiHint = "Generate an error - primarily for debugging",
    Icon = DataSourceIcons.Warning,
    Type = DataSourceType.Debug,
    Audience = Audience.Advanced,
    NameId = "e19ee6c4-5209-4c3d-8ae1-f4cbcf875c0a"   // namespace or guid
)]
[PublicApi]
public class Error: DataSourceBase
{
    /// <summary>
    /// The error title. Defaults to "Demo Error"
    /// </summary>
    public string Title { get; set; } = "Demo Error";

    /// <summary>
    /// The error message. Defaults to "Demo message of the Error DataSource"
    /// </summary>
    public string Message { get; set; } = "Demo message of the Error DataSource";

    /// <summary>
    /// Delay the result by this many seconds.
    /// </summary>
    /// <remarks>
    /// Meant for testing scenarios where the error needs time - like waiting for a connection timeout.
    /// Default / fallback is 0 seconds, which means no delay.
    /// </remarks>
    [Configuration(Fallback = 0, CacheRelevant = false)]
    public int DelaySeconds => Configuration.GetThis(0);

    /// <summary>
    /// Constructor to tell the system what out-streams we have.
    /// In this case it's just the "Default" containing a fake exception.
    /// </summary>
    public Error(MyServices services) : base(services, $"{DataSourceConstantsInternal.LogPrefix}.Error")
        => ProvideOut(GenerateExceptionStream);

    private IImmutableList<IEntity> GenerateExceptionStream()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        l.A("This is a fake Error / Exception");
        l.A("The Error DataSource creates an exception on purpose, to test exception functionality in Visual Query");
        var errToReturn = Error.Create(title: Title, message: Message);

        if (DelaySeconds <= 0)
            return l.Return(errToReturn, "fake error");

        // Delay the result by this many seconds.
        l.A($"Delaying result by {DelaySeconds} seconds");
        Thread.Sleep(DelaySeconds * 1000);

        return l.Return(errToReturn, "fake error");
    }
}