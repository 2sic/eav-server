using ToSic.Eav.Data;

namespace ToSic.Eav.Run
{
    public abstract class ServerPathsBase: IServerPaths
    {
        public abstract string FullAppPath(string virtualPath);

        public abstract string FullContentPath(string virtualPath);

        public string FullPathOfReference(string fileReference)
        {
            if (string.IsNullOrWhiteSpace(fileReference)) return fileReference;

            var parts = new ValueConverterBase.LinkParts(fileReference);
            if (parts.IsPage) return fileReference;

            return FullPathOfReference(parts.Id);
        }

        protected abstract string FullPathOfReference(int id);
    }
}
