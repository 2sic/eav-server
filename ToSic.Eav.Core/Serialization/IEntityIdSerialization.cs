﻿namespace ToSic.Eav.Serialization;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IEntityIdSerialization
{
    /// <summary>
    /// Should the ID be included
    /// </summary>
    bool? SerializeId { get; }
        
    /// <summary>
    /// Should the GUID be included
    /// </summary>
    bool? SerializeGuid { get; }
        
    /// <summary>
    /// Should the Title be included
    /// </summary>
    bool? SerializeTitle { get; }
}