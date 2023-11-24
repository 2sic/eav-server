using System.Collections.Generic;

namespace ToSic.Eav.Persistence.Efc.Models;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class ToSicEavAssignmentObjectTypes
{
    public ToSicEavAssignmentObjectTypes()
    {
        ToSicEavEntities = new HashSet<ToSicEavEntities>();
    }

    public int AssignmentObjectTypeId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public virtual ICollection<ToSicEavEntities> ToSicEavEntities { get; set; }
}