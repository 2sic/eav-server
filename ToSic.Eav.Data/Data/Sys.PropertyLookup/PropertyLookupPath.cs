namespace ToSic.Eav.Data.Sys.PropertyLookup;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class PropertyLookupPath(List<string>? original = null)
{
    public List<string> Parts = original == null
        ? []
        : [..original];

}
