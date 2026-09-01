using ToSic.Eav.Models;

namespace ToSic.Eav.Data.ContentTypes;

// TODO: CREATE INTERFACE, THEN MAKE PRIVATE

public record DataStorageDecorator: ModelFromEntity
{
    public string StoreType => GetThis("");

    public bool SaveIsDisabled => GetThis(false);

    public long ItemsMax => GetThis(long.MaxValue);

    public string DataProcessingHandler => GetThis("");
}
