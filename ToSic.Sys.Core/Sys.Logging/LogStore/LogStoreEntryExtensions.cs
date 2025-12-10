namespace ToSic.Sys.Logging;
public static class LogStoreEntryExtensions
{
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
