﻿namespace ToSic.Sys.Logging;

partial class LogExtensionsInternal
{
    /// <summary>
    /// Rename this logger - usually used when a base-class has a logger, 
    /// but the inherited class needs a different name
    /// </summary>
    /// <remarks>
    /// limits the length to 6 chars to make the output readable
    /// </remarks>
    [PrivateApi]
    internal static void Rename(this ILog log, string name)
    {
        if (name == null) return;
        if (!(log is Log realLog)) return;
        try
        {
            var dot = name.IndexOf(".", StringComparison.Ordinal);
            realLog.Scope = dot > 0 ? name.Substring(0, Math.Min(dot, LogConstants.ScopeMaxLength)) : "";
            var rest = dot > 0 ? name.Substring(dot + 1) : name;
            realLog.Name = rest.Substring(0, Math.Min(rest.Length, LogConstants.NameMaxLength));
            // 2022-10-25 2dm disable this next line, doesn't look useful
            //realLog.Name = Name.Substring(0, Math.Min(Name.Length, LogConstants.MaxNameLen));
        }
        catch { /* ignore */ }
    }

}