namespace ToSic.Eav.Persistence.File;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class InternalAppLoader
{
    public static ILog LoadLog { get; internal set; } = null;
}