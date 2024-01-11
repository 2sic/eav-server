namespace ToSic.Eav.ImportExport.Internal;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
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