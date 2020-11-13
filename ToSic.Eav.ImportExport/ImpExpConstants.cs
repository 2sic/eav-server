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

        ///// <summary>
        ///// This type is registered here, because the data layer also needs it
        ///// It's not ideal
        ///// For use in other code, use the ToSic.Eav.Apps.AppConstants.TypeAppConfig instead.
        ///// </summary>
        //public const string TypeAppConfig = "2SexyContent-App";

    }
}
