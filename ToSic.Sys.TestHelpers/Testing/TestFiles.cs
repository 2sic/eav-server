using System.Reflection;

namespace ToSic.Eav.Testing;

public static class TestFiles
{
    public static string GetTestPath(string relativePath)
    {
#if NETFRAMEWORK
        var codeBaseUrl = new Uri(uriString: Assembly.GetExecutingAssembly().CodeBase);
#else
        var codeBaseUrl = new Uri(uriString: Assembly.GetExecutingAssembly().Location);
#endif
        var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
        var dirPath = Path.GetDirectoryName(codeBasePath);
        return Path.Combine(dirPath, relativePath);
    }
}