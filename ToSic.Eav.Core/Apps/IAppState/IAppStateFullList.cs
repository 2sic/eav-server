using System.Collections.Immutable;
using ToSic.Eav.Data;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Apps
{
    /// <summary>
    /// An own interface just for the list, to better design how much of the AppState is needed in specific cases
    /// </summary>
    public interface IAppStateFullList
    {
        IImmutableList<IEntity> List { get; }
    }
}
