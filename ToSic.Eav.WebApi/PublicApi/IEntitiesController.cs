﻿using System;
using System.Collections.Generic;
using System.Net.Http;

namespace ToSic.Eav.WebApi.PublicApi
{
    public interface IEntitiesController
    {
        IEnumerable<Dictionary<string, object>> List(int appId, string contentType);

        void Delete(string contentType, int id, int appId, bool force = false);

        void Delete(string contentType, Guid guid, int appId, bool force = false);

        HttpResponseMessage Json(int appId, int id, string prefix, bool withMetadata);

    }
}