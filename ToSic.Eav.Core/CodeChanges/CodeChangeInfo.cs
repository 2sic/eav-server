using System;

namespace ToSic.Eav.CodeChanges
{
    public class CodeChangeInfo : ICodeChangeInfo
    {
        private CodeChangeInfo(string nameId, Version from, Version to, string link = default, string message = default)
        {
            NameId = nameId;
            From = from;
            To = to;
            Link = link;
            Message = message;
        }

        private CodeChangeInfo(ICodeChangeInfo original, string nameId = default, string link = default,
            string message = default)
        {
            NameId = nameId ?? original.NameId;
            Link = link ?? original.Link;
            Message = message ?? original.Message;
        }
        

        public string NameId { get; }

        public Version From { get; }

        public Version To { get; }

        public string Link { get; }

        public string Message { get; }

        /// <summary>
        /// Create a new CodeChangeInfo with placeholders like {0} replaced.
        /// </summary>
        /// <param name="replacements"></param>
        /// <returns></returns>
        public CodeChangeInfo Replace(params object[] replacements) => new CodeChangeInfo(this,
            nameId: string.Format(NameId, replacements),
            message: string.Format(Message, replacements));

        public CodeChangeUse UsedAs(int appId = default, string specificId = default, string[] more = default)
            => new CodeChangeUse(this, appId: appId, specificId: specificId, more: more);

        public static CodeChangeInfo V05To17(string nameId, string link = default, string message = default) =>
            new CodeChangeInfo(nameId, new Version(5, 0), new Version(17, 0), link ?? "https://go.2sxc.org/brc-17", message);



        public static ICodeChangeInfo V13To17(string nameId, string link = default, string message = default) =>
            new CodeChangeInfo(nameId, new Version(13, 0), new Version(17, 0), link, message);

        public static ICodeChangeInfo V16To18(string nameId, string link = default, string message = default) =>
            new CodeChangeInfo(nameId, new Version(16, 0), new Version(17, 0), link, message);

        public static ICodeChangeInfo V13Removed(string nameId, string link = default) =>
            new CodeChangeInfo(nameId, new Version(13, 0), null, link);
    }
}
