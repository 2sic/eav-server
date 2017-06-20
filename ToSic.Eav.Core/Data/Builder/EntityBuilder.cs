using System;

namespace ToSic.Eav.Data.Builder
{
    /// <summary>
    /// Helper commands to build an entity
    /// Should only be used by internal system data handlers - not for "public" use
    /// </summary>
    public static class EntityBuilder
    {
        public static void SetGuid(this Entity entity, Guid newGuid) => entity.EntityGuid = newGuid;

        public static void SetTitleField(this Entity entity, string name) => entity.TitleFieldName = name;

        public static void SetMetadata(this Entity entity, Metadata meta) => entity.Metadata = meta;
    }
}
