using ToSic.Eav.Apps.Parts;

namespace ToSic.Eav.Apps
{
    /// <inheritdoc />
    /// <summary>
    /// Basic App-Reading System to access app data and read it
    /// </summary>
    public class AppRuntime : AppRuntimeBase
    {

        #region constructors

        public AppRuntime(MyServices services, string logName = null) : base(services, logName ?? "Eav.AppRt")
        {
        }

        #endregion
    }
}
