using ToSic.Eav.Data.Dimensions.Sys;

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