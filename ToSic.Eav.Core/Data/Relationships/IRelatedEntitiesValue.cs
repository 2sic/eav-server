using System.Collections;

namespace ToSic.Eav.Data;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IRelatedEntitiesValue
{
    IList Identifiers { get; }
}