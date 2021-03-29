using ToSic.Eav.DataSources.Queries;

namespace ToSic.Eav.DataSources
{
    [VisualQuery(
        NiceName = "Error DataSource",
        UiHint = "Generate an error - primarily for debugging",
        Icon = "warning",
        Difficulty = DifficultyBeta.Advanced,
        GlobalName = "e19ee6c4-5209-4c3d-8ae1-f4cbcf875c0a"   // namespace or guid
    )]
    public class Error: DataSourceBase
    {
        public override string LogId => "DS.Error";

        public string Title { get; set; } = "Demo Error";
        public string Message { get; set; } = "Demo message of the Error DataSource";
        
        /// <summary>
        /// Constructor to tell the system what out-streams we have
        /// </summary>
        public Error()
        {
            Provide(() => SetException(Title, Message)); 
        }

    }
}
