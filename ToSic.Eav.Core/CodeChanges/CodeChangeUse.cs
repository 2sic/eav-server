using System;

namespace ToSic.Eav.CodeChanges
{
    public class CodeChangeUse: ICodeChangeInfo
    {
        public string SpecificId { get; }
        public string[] More { get; }

        public CodeChangeUse(ICodeChangeInfo change, int appId = default, string specificId = default, string[] more = default)
        {
            SpecificId = specificId;
            More = more;
            Change = change;
            AppId = appId;
        }

        public readonly ICodeChangeInfo Change;
        public readonly int AppId;
        public string NameId => Change.NameId;

        public Version From => Change.From;

        public Version To => Change.To;

        public string Link => Change.Link;

        public string Message => Change.Message;

        public CodeChangeUse UsedAs(int appId = default, string specificId = default,
            string[] more = default)
            => new CodeChangeUse(this,
                appId: appId == default ? AppId : appId,
                specificId: specificId ?? SpecificId,
                more: more ?? More);

    }
}
