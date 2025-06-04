namespace ToSic.Eav.ImportExport.Internal;

[ShowApiWhenReleased(ShowApiMode.Never)]
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