namespace ToSic.Eav.Serialization
{
    public class SubEntitySerialization: ISubEntitySerialization
    {
        public bool? Serialize { get; set; }
        
        public bool? SerializeId { get; set; }

        public bool? SerializeGuid { get; set; }

        public bool? SerializeTitle { get; set; }


        public static ISubEntitySerialization Stabilize(ISubEntitySerialization original, bool items, bool id, bool guid, bool title) =>
            new SubEntitySerialization
            {
                Serialize = original?.Serialize ?? items,
                SerializeId = original?.SerializeId ?? id,
                SerializeGuid = original?.SerializeGuid ?? guid,
                SerializeTitle = original?.SerializeTitle ?? title
            };
    }
}
