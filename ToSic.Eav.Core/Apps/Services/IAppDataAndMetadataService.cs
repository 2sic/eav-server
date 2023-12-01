using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps.Services
{
    public interface IAppDataAndMetadataService: IAppDataService //, IMetadataOfSource
    {
        public IMetadataOf Metadata { get; }
    }
}
