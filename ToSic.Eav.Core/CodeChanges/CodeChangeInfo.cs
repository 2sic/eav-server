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
        

        public string NameId { get; }

        public Version From { get; }

        public Version To { get; }

        public string Link { get; }

        public string Message { get; }

        public CodeChangeUse UsedAs(int appId = default, string specificId = default, string[] more = default)
            => new CodeChangeUse(this, appId: appId, specificId: specificId, more: more);

        public static ICodeChangeInfo V13To17(string nameId, string link = default, string message = default) =>
            new CodeChangeInfo(nameId, new Version(13, 0), new Version(17, 0), link, message);

        public static ICodeChangeInfo V16To18(string nameId, string link = default, string message = default) =>
            new CodeChangeInfo(nameId, new Version(16, 0), new Version(17, 0), link, message);

        public static ICodeChangeInfo V13Removed(string nameId, string link = default) =>
            new CodeChangeInfo(nameId, new Version(13, 0), null, link);
    }
}
