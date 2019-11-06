﻿using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Metadata;
using IDataSource = ToSic.Eav.DataSources.IDataSource;

namespace ToSic.Eav.Apps.Interfaces
{
    public interface IAppData: IDataSource, IDataTarget
    {
        void Create(string contentTypeName, Dictionary<string, object> values, string userName = null, MetadataFor target = null);
        void Create(string contentTypeName, IEnumerable<Dictionary<string, object>> multiValues, string userName = null);

        void Update(int entityId, Dictionary<string, object> values, string userName = null);

        void Delete(int entityId, string userName = null);

        /// <summary>
        /// Metadata is an important feature of apps. <br/>
        /// So the App DataSource automatically provides direct access to the metadata system.
        /// This allows users of the App to query metadata directly through this object. 
        /// </summary>
        IMetadataProvider Metadata { get; }
    }
}
