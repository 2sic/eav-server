﻿using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// A DataSource that passes through all In Connections. Can be used con consolidate/merge multiple Sources into one.
	/// </summary>
	[PublicApi_Stable_ForUseInYourCode]

	[VisualQuery(GlobalName = "ToSic.Eav.DataSources.PassThrough, ToSic.Eav.DataSources",
        Type = DataSourceType.Source, DynamicOut = true)]

    public class PassThrough : DataSourceBase
	{
        /// <inheritdoc/>
        [PrivateApi]
        public override string LogId => "DS.PasThr";

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new PassThrough DataSources
        /// </summary>
        [PrivateApi]
        public PassThrough()
		{
            Out = In;
        }

        /// <summary>
        /// provide a static cachekey - as there is nothing dynamic on this source to modify the cache
        /// </summary>
        /// <remarks>
        /// if the key is not static (like the default setup) it will always cause errors
        /// </remarks>
	    public override string CachePartialKey => "PassThrough";
	}
}