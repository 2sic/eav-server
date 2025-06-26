using ToSic.Eav.Data.Sys.Dimensions;

namespace ToSic.Eav.Persistence.Efc.Sys.DbModels;

public partial class TsDynDataDimension
{
    public DimensionDefinition AsDimensionDefinition
        => new()
        {
            DimensionId = DimensionId,
            Parent = Parent,
            Name = Name,
            Key = Key,
            EnvironmentKey = EnvironmentKey,
            Active = Active,
        };
}