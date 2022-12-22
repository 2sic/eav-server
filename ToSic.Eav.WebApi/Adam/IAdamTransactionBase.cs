using System;

// TODO: PROBABLY MOVE TO 2SXC

namespace ToSic.Eav.WebApi.Adam
{
    public interface IAdamTransactionBase
    {
        void Init(int appId, string contentType, Guid itemGuid, string field, bool usePortalRoot);
    }
}