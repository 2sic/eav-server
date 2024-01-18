namespace ToSic.Eav.Persistence.File;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class InternalAppLoader
{
    public static ILog LoadLog { get; internal set; } = null;
}