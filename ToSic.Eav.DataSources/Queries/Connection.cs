using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources.Queries
{
    /// <summary>
    /// Represent a connection which connects two DataSources in a Query
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public struct Connection
    {
        internal const string FromField = "From";
        internal const string OutField = "Out";
        internal const string ToField = "To";
        internal const string InField = "In";

        /// <summary>
        /// The DataSource ID which has data, an <see cref="IDataSource"/>
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// The Out-stream name of the Source DataSource
        /// </summary>
        public string Out { get; set; }

        /// <summary>
        /// The DataSource ID which receives data, an <see cref="IDataTarget"/>
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// The In-stream name on the target DataSource
        /// </summary>
        public string In { get; set; }

        [PrivateApi]
        public override string ToString() => From + ":" + Out + ">" + To + ":" + In;
    }
}
