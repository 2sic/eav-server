using System.Linq;
using System.Text;

namespace ToSic.Lib.Logging;

// ReSharper disable once InconsistentNaming
partial class ILogExtensions
{
    /// <summary>
    /// Helper to dump a byte array to the log
    /// </summary>
    /// <param name="_"></param>
    /// <param name="bytes"></param>
    /// <returns></returns>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static string Dump(this ILog _, byte[]? bytes)
    {
        if (bytes == null || !bytes.Any())
            return "[](0)";
        return $"[{Encoding.Default.GetString(bytes)}]({bytes.Length})";
    }
}