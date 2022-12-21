using System;
using ToSic.Eav.Run.Unknown;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Data
{
    public class ValueConverterUnknown: ValueConverterBase
    {
        public ValueConverterUnknown(WarnUseOfUnknown<ValueConverterUnknown> warn) : base (LogConstants.FullNameUnknown) { }

        protected override string ResolveFileLink(int linkId, Guid itemGuid) => $"file-link-resolved-for-id-{linkId}-in{itemGuid}";

        protected override string ResolvePageLink(int id) => $"page-link-resolved-for-{id}";
    }
}
