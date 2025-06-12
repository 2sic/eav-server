using ToSic.Eav.Data.Dimensions.Sys;

namespace ToSic.Eav.Persistence.Efc.Sys.DbModels;

public partial class TsDynDataDimension : DimensionDefinition
{
    /// <summary>
    /// Compares two keys to see if they are the same.
    /// </summary>
    /// <param name="environmentKey"></param>
    /// <returns></returns>
    public bool Matches(string environmentKey)
        => string.Equals(EnvironmentKey, environmentKey, StringComparison.InvariantCultureIgnoreCase);

}