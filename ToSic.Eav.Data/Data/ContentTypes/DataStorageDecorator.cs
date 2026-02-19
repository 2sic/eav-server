using ToSic.Eav.Models;

namespace ToSic.Eav.Data.ContentTypes;

public record DataStorageDecorator: ModelOfEntity
{
    public string StoreType => GetThis("");

    public bool SaveIsDisabled => GetThis(false);

    public long ItemsMax => GetThis(long.MaxValue);

    public string DataProcessingHandler => GetThis("");
}
