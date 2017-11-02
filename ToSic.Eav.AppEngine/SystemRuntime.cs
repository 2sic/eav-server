using ToSic.Eav.Interfaces;

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
        public static int MetadataType(string typeName) => Factory.Resolve<IGlobalMetadataProvider>().GetType(typeName);

        public static string MetadataType(int typeNumber) => Factory.Resolve<IGlobalMetadataProvider>().GetType(typeNumber);

    }
}
