namespace ToSic.Eav.Serialization
{
    public class SubEntitySerialization: ISubEntitySerialization
    {
        public bool? Serialize { get; set; }
        
        public bool? SerializeId { get; set; }

        public bool? SerializeGuid { get; set; }

        public bool? SerializeTitle { get; set; }


        public static ISubEntitySerialization Stabilize(ISubEntitySerialization original,
            bool serialize = false, bool id = false, bool guid = false, bool title = false) =>
            new SubEntitySerialization
            {
                Serialize = original?.Serialize ?? serialize,
                SerializeId = original?.SerializeId ?? id,
                SerializeGuid = original?.SerializeGuid ?? guid,
                SerializeTitle = original?.SerializeTitle ?? title
            };
        public static ISubEntitySerialization Stabilize(
            ISubEntitySerialization original, 
            ISubEntitySerialization addition = null, 
            bool serialize = false, bool id = false, bool guid = false, bool title = false) =>
            new SubEntitySerialization
            {
                Serialize = original?.Serialize ?? addition?.Serialize ?? serialize,
                SerializeId = original?.SerializeId ?? addition?.SerializeId ?? id,
                SerializeGuid = original?.SerializeGuid ?? addition?.SerializeGuid ?? guid,
                SerializeTitle = original?.SerializeTitle ?? addition?.SerializeTitle ?? title
            };
    }
}
