namespace ToSic.Eav.Apps.Sys.State;

partial class AppState
{
    #region Load Status and Statistics

    /// <summary>
    /// Shows that the app is loading / building up the data.
    /// </summary>
    public bool Loading { get; private set; }

    /// <summary>
    /// Shows that the initial load has completed
    /// </summary>
    public bool FirstLoadCompleted { get; private set; }

    /// <summary>
    /// Show how many times the app has been Dynamically updated - in case we run into cache rebuild problems.
    /// </summary>
    public int DynamicUpdatesCount { get; private set; }

    #endregion

}