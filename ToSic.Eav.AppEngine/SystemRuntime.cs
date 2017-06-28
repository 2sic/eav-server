namespace ToSic.Eav.Apps
{
    public class SystemRuntime
    {
        /// <summary>
        /// Retrieve the Assignment-Type-ID which is used to determine which type of object
        /// an entity is assigned to (because just the object ID would be ambiguous)
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static int GetMetadataType(string typeName) => DataSource.GetCache(null).GetMetadataType(typeName);

        public static string GetMetadataType(int typeNumber) => DataSource.GetCache(null).GetMetadataType(typeNumber);

    }
}
