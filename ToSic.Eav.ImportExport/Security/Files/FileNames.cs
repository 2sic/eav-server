using System.IO;
using System.Text.RegularExpressions;

namespace ToSic.Eav.Security.Files;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class FileNames
{
    /// <summary>
    /// For validating upload requests - to prevent unwanted files being uploaded
    /// </summary>
    public const string RiskyExtensionsUpload =
        @"^\.\s*(ade|adp|app|bas|bat|chm|class|cmd|com|cpl|crt|dll|exe|fxp|hlp|hta|ins|isp|jse|lnk|mda|mdb|mde|mdt|mdw|mdz|msc|msi|msp|mst|ops|pcd|pif|prf|prg|reg|scf|scr|sct|shb|shs|url|vb|vbe|vbs|wsc|wsf|wsh|cshtml|vbhtml|cs|ps[0-9]|ascx|aspx|asmx|config|inc|js|html|sql|bin|iso|asp|sh|php([0-9])?|pl|cgi|386|torrent|jar|vbscript|cer|csr|jsp|drv|sys|csh|inf|htaccess|htpasswd|ksh)\s*$";
    private static readonly Regex RiskyUploadDetector = new(RiskyExtensionsUpload);

    /// <summary>
    /// For validating download requests - so we don't offer files as download which shouldn't be downloaded. 
    /// </summary>
    /// <remarks>
    /// It's the same like RiskyUploads, except
    /// - js is allowed
    /// - html is allowed
    /// </remarks>
    public const string RiskyExtensionsDownload =
        @"^\.\s*(ade|adp|app|bas|bat|chm|class|cmd|com|cpl|crt|dll|exe|fxp|hlp|hta|ins|isp|jse|lnk|mda|mdb|mde|mdt|mdw|mdz|msc|msi|msp|mst|ops|pcd|pif|prf|prg|reg|scf|scr|sct|shb|shs|url|vb|vbe|vbs|wsc|wsf|wsh|cshtml|vbhtml|cs|ps[0-9]|ascx|aspx|asmx|config|inc|sql|bin|iso|asp|sh|php([0-9])?|pl|cgi|386|torrent|jar|vbscript|cer|csr|jsp|drv|sys|csh|inf|htaccess|htpasswd|ksh)\s*$";
    public static readonly Regex RiskyDownloadDetector = new(RiskyExtensionsDownload);


    /// <summary>
    /// For extra checks if something looks like a code file
    /// </summary>
    public static readonly string RiskyCodeExtensions =
        @"^\.\s*(dll|exe|hta|vb|vbe|vbs|wsc|wsf|wsh|cshtml|vbhtml|cs|ps[0-9]|ascx|aspx|asmx|config|inc|bin|asp|sh|php([0-9])?|pl|cgi|vbscript|cer|csr|jsp|htaccess|htpasswd|ksh)\s*$";
    private static readonly Regex CodeDetector = new(RiskyCodeExtensions);


    /// <summary>
    /// Safe char, used to replace bad chars
    /// </summary>
    public const string SafeChar = "_";


    /// <summary>
    /// Restricted words for file names in windows
    /// </summary>
    public static readonly List<string> RestrictedFileNames =
    [
        "CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4",
        "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4",
        "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    ];


    public static bool IsKnownRiskyExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        return !string.IsNullOrEmpty(extension) && RiskyUploadDetector.IsMatch(extension);
    }

    public static bool IsKnownCodeExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        return !string.IsNullOrEmpty(extension) && CodeDetector.IsMatch(extension);
    }

    public static string SanitizeFileName(string fileName)
    {
        // make sure the file name doesn't contain injected path-traversal
        fileName = Path.GetFileName(fileName);

        // handle null, empty and white space
        if (string.IsNullOrWhiteSpace(fileName)) return SafeChar;

        // remove forbidden / troubling file name characters
        fileName = fileName
            .Replace("+", "plus")
            .Replace("%", "per")
            .Replace("#", "hash");

        // replace invalid file name chars
        // https://stackoverflow.com/questions/309485/c-sharp-sanitize-file-name
        fileName = string.Join(SafeChar,
            fileName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));

        // ensure that fileName is not restricted 
        if (RestrictedFileNames.Contains(fileName, StringComparer.OrdinalIgnoreCase))
            return SafeChar;

        return fileName;
    }
}