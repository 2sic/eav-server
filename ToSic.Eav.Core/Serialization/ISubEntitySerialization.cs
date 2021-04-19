namespace ToSic.Eav.Serialization
{
    public interface ISubEntitySerialization : IEntityIdSerialization
    {
        /// <summary>
        /// Should sub entities get serialized?
        /// </summary>
        bool? Serialize { get; }
    }
}
