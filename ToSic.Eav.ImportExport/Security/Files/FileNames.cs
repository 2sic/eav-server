using System.IO;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Security.Files
{
    public class FileNames
    {
        /// <summary>
        /// For validating upload requests - to prevent unwanted files being uploaded
        /// </summary>
        public const string RiskyExtensionsUpload =
            @"^\.\s*(ade|adp|app|bas|bat|chm|class|cmd|com|cpl|crt|dll|exe|fxp|hlp|hta|ins|isp|jse|lnk|mda|mdb|mde|mdt|mdw|mdz|msc|msi|msp|mst|ops|pcd|pif|prf|prg|reg|scf|scr|sct|shb|shs|url|vb|vbe|vbs|wsc|wsf|wsh|cshtml|vbhtml|cs|ps[0-9]|ascx|aspx|asmx|config|inc|js|html|sql|bin|iso|asp|sh|php([0-9])?|pl|cgi|386|torrent|jar|vbscript|cer|csr|jsp|drv|sys|csh|inf|htaccess|htpasswd|ksh)\s*$";
        private static readonly Regex RiskyUploadDetector = new Regex(RiskyExtensionsUpload);

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
        public static readonly Regex RiskyDownloadDetector = new Regex(RiskyExtensionsDownload);


        /// <summary>
        /// For extra checks if something looks like a code file
        /// </summary>
        public static readonly string RiskyCodeExtensions =
            @"^\.\s*(dll|exe|hta|vb|vbe|vbs|wsc|wsf|wsh|cshtml|vbhtml|cs|ps[0-9]|ascx|aspx|asmx|config|inc|bin|asp|sh|php([0-9])?|pl|cgi|vbscript|cer|csr|jsp|htaccess|htpasswd|ksh)\s*$";
        private static readonly Regex CodeDetector = new Regex(RiskyCodeExtensions);

        
        
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


    }
}
