namespace ToSic.Eav.Apps;

partial class AppState
{
    /// <summary>
    /// Shows that the app is loading / building up the data.
    /// </summary>
    private bool Loading;

    /// <summary>
    /// Shows that the initial load has completed
    /// </summary>
    private bool FirstLoadCompleted;

    /// <summary>
    /// Show how many times the app has been Dynamically updated - in case we run into cache rebuild problems.
    /// </summary>
    public int DynamicUpdatesCount { get; private set; }
    
}