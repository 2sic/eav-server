using System;

namespace ToSic.Eav.Identity
{
    public static class Mapper
    {
        public static string GuidCompress(Guid newGuid)
        {
            var modifiedBase64 = System.Convert.ToBase64String(newGuid.ToByteArray())
                .Replace('+', '-').Replace('/', '_')    // avoid invalid URL characters
                .Substring(0, 22);                      // truncate trailing "==" characters
            return modifiedBase64;
        }

        public static Guid GuidRestore(string shortGuid)
        {
            var base64 = shortGuid.Replace('-', '+').Replace('_', '/') + "==";
            var bytes = System.Convert.FromBase64String(base64);
            return new Guid(bytes);
        }

    }
}