using System;
using ToSic.Lib.Logging;

namespace ToSic.Eav.WebApi.Adam
{
    public interface IAdamTransactionBase
    {
        void Init(int appId, string contentType, Guid itemGuid, string field, bool usePortalRoot, ILog parentLog);
    }
}