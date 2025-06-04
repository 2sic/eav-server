using ToSic.Lib.Services;

namespace ToSic.Eav.Data.ValueConverter.Sys;

/// <summary>
/// Trivial value converter - doesn't convert anything.
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class ValueConverterBase(string logName) : ServiceBase(logName), IValueConverter
{
    public const string PrefixPage = "page";
    public const string PrefixFile = "file";
    public const string Separator = ":";


    public virtual string ToReference(string value) => value;

    public virtual string ToValue(string reference, Guid itemGuid = default) => reference;

    protected abstract string ResolveFileLink(int linkId, Guid itemGuid);

    protected abstract string ResolvePageLink(int id);

    /// <summary>
    /// Optionally log exceptions - if not implemented, won't log
    /// </summary>
    protected virtual void LogConversionExceptions(string originalValue, Exception e) { }

    public static bool CouldBeReference(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference)) return false;
        // must contain ":"
        if (!reference.Contains(Separator)) return false;
        // Avoid false positives on full paths
        if (reference.Contains("/") || reference.Contains("\\")) return false;
        // minimum "page:#" or "file:#"
        if (reference.Length < 6) return false; 
        return true;
    }

    protected string TryToResolveCodeToLink(Guid itemGuid, string originalValue)
    {
        try
        {
            if (string.IsNullOrEmpty(originalValue)) return originalValue;

            var parts = new LinkParts(originalValue);

            if (!parts.IsMatch) return originalValue;

            var result = (parts.IsPage
                             ? ResolvePageLink(parts.Id)
                             : ResolveFileLink(parts.Id, itemGuid))
                         ?? originalValue;

            return result + (result == originalValue ? "" : parts.Params);
        }
        catch (Exception e)
        {
            LogConversionExceptions(originalValue, e);
            return originalValue;
        }
    }

}