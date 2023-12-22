using System;
using ToSic.Eav.Internal.Unknown;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Data;

internal class ValueConverterUnknown: ValueConverterBase
{
    public ValueConverterUnknown(WarnUseOfUnknown<ValueConverterUnknown> _) : base (LogConstants.FullNameUnknown) { }

    protected override string ResolveFileLink(int linkId, Guid itemGuid) => $"file-link-resolved-for-id-{linkId}-in{itemGuid}";

    protected override string ResolvePageLink(int id) => $"page-link-resolved-for-{id}";
}