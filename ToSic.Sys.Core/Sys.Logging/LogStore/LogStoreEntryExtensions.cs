namespace ToSic.Sys.Logging;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class LogStoreEntryExtensions
{
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static void TryUpdateSpecs(this LogStoreEntry? entry, Func<IDictionary<string, string>> specsGenerator)
    {
        if (entry == null)
            return;

        try
        {
            var specs = specsGenerator();
            entry.UpdateSpecs(specs);
        }
        catch
        {
            // ignore errors when generating specs
        }
    }
}
