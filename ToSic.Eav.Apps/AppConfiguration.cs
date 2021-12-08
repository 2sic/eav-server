using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// Configuration values used by the runtime as the app is in use
    /// </summary>
    [PrivateApi("not an ideal place yet, will move some time")]
    public class Configuration
    {
        // todo: move to some kind of injection thingy?

        /// <summary>
        /// The type name used to store templates in the eav-system
        /// </summary>
        public static string TemplateContentType = Eav.ImportExport.Settings.TemplateContentType;
    }
}
