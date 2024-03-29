﻿using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSource.Internal;

internal static class DataSourceHelpers
{
    public static T Init<T>(this T thisDs, ILookUpEngine lookUpEngine) where T : IDataSource
    {
        if (lookUpEngine != null && thisDs.Configuration is DataSourceConfiguration dsConfig)
            dsConfig.LookUpEngine = lookUpEngine;
        return thisDs;
    }
}