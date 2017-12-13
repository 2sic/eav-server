using ToSic.Eav.DataSources.VisualQuery;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// A DataSource that passes through all In Connections. Can be used con consollidate/merge multiple Sources into one.
	/// </summary>

	[VisualQuery(GlobalName = "ToSic.Eav.DataSources.PassThrough, ToSic.Eav.DataSources",
        Type = DataSourceType.Source, DynamicOut = true)]

    public class PassThrough : BaseDataSource
	{
	    public override string LogId => "DS.Passth";

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new PassThrough DataSources
        /// </summary>
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