
namespace ToSic.Eav.CodeChanges
{
    public class CodeChangeUse
    {
        public CodeChangeUse(ICodeChangeInfo change, int appId = default, string specificId = default, string[] more = default)
        {
            Change = change;
            SpecificId = specificId;
            More = more;
            AppId = appId;
        }

        public ICodeChangeInfo Change { get; }
        public int AppId { get; }   // FYI: ATM not reported anywhere
        public string SpecificId { get; }
        public string[] More { get; }


        public CodeChangeUse UsedAs(int appId = default, string specificId = default, string[] more = default)
            => new CodeChangeUse(Change,
                appId: appId == default ? AppId : appId,
                specificId: specificId ?? SpecificId,
                more: more ?? More);

    }
}
