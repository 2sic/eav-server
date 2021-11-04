﻿namespace ToSic.Eav.Serialization
{
    public class MetadataForSerialization
    {
        public bool? Serialize { get; set; }

        public bool? SerializeKey { get; set; }
        public bool? SerializeType { get; set; }



        public static MetadataForSerialization Stabilize(
            MetadataForSerialization original,
            MetadataForSerialization addition = null,
            bool serialize = false, bool key = false, bool type = false) =>
            new MetadataForSerialization
            {
                Serialize = original?.Serialize ?? addition?.Serialize ?? serialize,
                SerializeKey = original?.SerializeKey ?? addition?.SerializeKey ?? key,
                SerializeType = original?.SerializeType ?? addition?.SerializeType ?? type,
            };

    }
}