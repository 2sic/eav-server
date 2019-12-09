using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources.Queries
{
    [PrivateApi]
    public interface IQuery: IDataSource
    {
        /// <summary>
        /// The underlying definition for the current query so you can check what's inside.
        /// </summary>
        QueryDefinition Definition { get; }

        /// <summary>
        /// Add/Set a parameter for the query, which will be used by the [Params:Xxx] tokens.
        /// </summary>
        /// <param name="key">Key - the part used in [Params:key]</param>
        /// <param name="value">The value it will resolve to. Can also be another token.</param>
        void Param(string key, string value);
    }
}
