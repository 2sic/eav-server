namespace ToSic.Eav.ImportExport
{
    public class ImpExpConstants
    {
        // ReSharper disable InconsistentNaming
        public enum Files
        {
            json
        }
        // ReSharper restore InconsistentNaming

        public static string Extension(Files ext) => $".{ext}";
    }
}
