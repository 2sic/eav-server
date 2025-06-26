using System.Security.Cryptography;

namespace ToSic.Sys.Security.Encryption;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class Rfc2898Generator
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="config"></param>
    /// <remarks>
    /// Up till v19.03-02 this was only implemented for .net Framework, because previously the build was for .net standard 2.0 (which didn't support some of these features).
    /// But since we're not compiling for .net standard anymore - but for .net core 9, we can now implement this for all platforms.
    /// https://github.com/2sic/2sxc/issues/3594
    /// </remarks>
    /// <returns></returns>
    public virtual byte[] GetKeyBytes(AesConfiguration config)
    {
//#if NETFRAMEWORK
//        // Note: as of .net standard 2.0 we can't specify the HashAlgorithm
//        // This is why this is only implemented for .net Framework
//        // For Oqtane, it must be implemented there
//        // #cleanUpForNetStandard2.1
//        var pwBytes = new Rfc2898DeriveBytes(config.Password, config.SaltBytes(), config.KeyGenIterations, HashAlgorithmName.SHA256);
//        var keyNew = pwBytes.GetBytes(config.KeySize / 8);
//        return keyNew;
//#else
        var pwBytes = new Rfc2898DeriveBytes(config.Password, config.SaltBytes(), config.KeyGenIterations, HashAlgorithmName.SHA256);
        var keyNew = pwBytes.GetBytes(config.KeySize / 8);
        return keyNew;
//#endif
    }
}