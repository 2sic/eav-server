using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Internal DataSource to generate an error on purpose.
    /// This is to test / verify error handling in VisualQuery. See also [](xref:Basics.Query.Debug.Index)
    /// </summary>
    /// <remarks>
    /// In advanced programming scenarios you can also use this DataSource instead of another one to provide a stream of errors.
    /// </remarks>
    [VisualQuery(
        NiceName = "Error DataSource",
        UiHint = "Generate an error - primarily for debugging",
        Icon = "warning",
        Difficulty = DifficultyBeta.Advanced,
        GlobalName = "e19ee6c4-5209-4c3d-8ae1-f4cbcf875c0a"   // namespace or guid
    )]
    [PublicApi]
    public class Error: DataSourceBase
    {
        [PrivateApi]
        public override string LogId => "DS.Error";

        /// <summary>
        /// The error title. Defaults to "Demo Error"
        /// </summary>
        public string Title { get; set; } = "Demo Error";

        /// <summary>
        /// The error message. Defaults to "Demo message of the Error DataSource"
        /// </summary>
        public string Message { get; set; } = "Demo message of the Error DataSource";
        
        /// <summary>
        /// Constructor to tell the system what out-streams we have.
        /// In this case it's just the "Default" containing a fake exception.
        /// </summary>
        public Error() => Provide(GenerateExceptionStream);

        private ImmutableArray<IEntity> GenerateExceptionStream()
        {
            var wrapLog = Log.Call<ImmutableArray<IEntity>>();
            Log.Add("This is a fake Error / Exception");
            Log.Add("The Error DataSource creates an exception on purpose, to test exception functionality in Visual Query");
            return wrapLog("fake error", SetError(Title, Message));
        }
    }
}
