using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// A DataSource that forwards all `In` Connections. It's more for internal use.
	/// </summary>
	[PublicApi_Stable_ForUseInYourCode]

	[VisualQuery(
        NiceName = "Pass-Through",
        UiHint = "Technical DataSource, doesn't do anything",
        Icon = Icons.CopyAll,
        Type = DataSourceType.Source, 
        Difficulty = DifficultyBeta.Advanced,
        GlobalName = "ToSic.Eav.DataSources.PassThrough, ToSic.Eav.DataSources",
        DynamicOut = true,
        DynamicIn = true)]

    public class PassThrough : DataSource
	{
        /// <inheritdoc />
        /// <summary>
        /// Constructs a new PassThrough DataSources
        /// </summary>
        [PrivateApi]
        public PassThrough(Dependencies services) : this(services, $"{DataSourceConstants.LogPrefix}.PasThr")
        {
        }

        [PrivateApi]
        protected PassThrough(Dependencies services, string logName) : base(services, logName)
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