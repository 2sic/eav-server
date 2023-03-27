using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSource
{
    /// <summary>
    /// Public interface for an Eav DataSource.
    /// All DataSource objects are based on this.
    /// It is based on both the <see cref="IDataSourceSource"/> and <see cref="IDataSourceTarget"/>
    /// </summary>
    [PublicApi]
	public interface IDataSource: IDataSourceSource, IDataSourceTarget, IDataSourceLinkable
    {
    }

}
