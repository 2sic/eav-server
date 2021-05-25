using System.Collections.Generic;
using ToSic.Eav.Documentation;
using static ToSic.Eav.Constants;

namespace ToSic.Eav.Data
{
    [PrivateApi]
    public class FieldConstants
    {
        public static Dictionary<string, string> ReservedNames { get; } = new Dictionary<string, string>
        {
            { EntityFieldTitle, "This is a longer, unique name for the entity title."},
            { EntityFieldId, "This is a longer, unique name for the entity Id."},
            { EntityFieldGuid, "This is a longer, unique name for the entity Guid."},
            { EntityFieldType, "This is a longer, unique name for the entity type name."},
            { EntityFieldIsPublished, "This is a property which tells the system if the entity is published (and not draft)."},
            { EntityFieldCreated, "This is an internal field which tells us when the entity was created."},
            { EntityFieldModified, "This is an internal field which tells us when the entity was last modified."},
            { "for", "This is an internal information which tells us if the entity is metadata for something."},
            { "metadata", "This is usually a property on the entity which tells us about additional metadata of this entity."},
        };
    }
}
