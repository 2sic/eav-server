using ToSic.Eav.Documentation;

namespace ToSic.Eav.Serialization
{
    /// <summary>
    /// Experimental v11.13 / 2021-03
    /// An entity should be able to specify if some properties should not be included
    /// </summary>
    [PrivateApi]
    public interface IEntitySerialization: IEntityIdSerialization
    {
        bool? SerializeModified { get; }

        bool? SerializeCreated { get; }
        
        bool RemoveEmptyStringValues { get; }
        bool RemoveNullValues { get; }
        bool RemoveZeroValues { get; }

        bool RemoveBoolFalseValues { get; }

        MetadataForSerialization SerializeMetadataFor { get; }

        ISubEntitySerialization SerializeMetadata { get; }
        
        ISubEntitySerialization SerializeRelationships { get; }

    }

}
