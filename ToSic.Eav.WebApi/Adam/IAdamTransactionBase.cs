﻿using System;
using ToSic.Eav.Logging;

namespace ToSic.Eav.WebApi.Adam
{
    public interface IAdamTransactionBase
    {
        void Init(int appId, string contentType, Guid itemGuid, string field, bool usePortalRoot, ILog parentLog);
    }
}