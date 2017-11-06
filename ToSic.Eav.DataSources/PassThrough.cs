using ToSic.Eav.DataSources.Attributes;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// A DataSource that passes through all In Connections. Can be used con consollidate/merge multiple Sources into one.
	/// </summary>
	[PipelineDesigner]
	[DataSourceProperties(Type = DataSourceType.Source, DynamicOut = true)]

    public class PassThrough : BaseDataSource
	{
	    public override string LogId => "DS.Passth";

        /// <summary>
        /// Constructs a new PassThrough DataSources
        /// </summary>
        public PassThrough()
		{
			Out = In;
		}
	}
}