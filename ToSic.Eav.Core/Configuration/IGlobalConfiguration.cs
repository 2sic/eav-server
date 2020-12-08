namespace ToSic.Eav.Configuration
{
    public interface IGlobalConfiguration
    {
        /// <summary>
        /// The absolute folder where the data is stored, usually ends in "...\.data"
        /// </summary>
        /// <returns>The folder, can be null if it was never set</returns>
        string DataFolder { get; set; }
    }
}