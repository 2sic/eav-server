using System;

namespace ToSic.Eav.Code.Infos
{
    public class CodeInfoObsolete: CodeInfo
    {
        private CodeInfoObsolete(string nameId, Version from, Version to, string link = default, string message = default) : base(CodeInfoTypes.Obsolete, nameId, from, to, link, message)
        {
        }

        public static ICodeInfo V05To17(string nameId, string link = default, string message = default) =>
            new CodeInfoObsolete(nameId, new Version(5, 0), new Version(17, 0), link ?? "https://go.2sxc.org/brc-17", message);

        public static ICodeInfo CaV8To17(string nameId, string link = default, string message = default) =>
            new CodeInfoObsolete(nameId, new Version(8, 0), new Version(17, 0), link, message);


        public static ICodeInfo V13To17(string nameId, string link = default, string message = default) =>
            new CodeInfoObsolete(nameId, new Version(13, 0), new Version(17, 0), link, message);

        public static ICodeInfo V16To18(string nameId, string link = default, string message = default) =>
            new CodeInfoObsolete(nameId, new Version(16, 0), new Version(18, 0), link, message);


    }
}
