namespace ToSic.Eav.Serialization
{
    public interface ISubEntitySerialization : IEntityIdSerialization
    {
        /// <summary>
        /// Should sub entities get serialized?
        /// </summary>
        bool? Serialize { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>New v15.03 WIP</remarks>
        bool? SerializesAsCsv { get; set; }
    }
}
