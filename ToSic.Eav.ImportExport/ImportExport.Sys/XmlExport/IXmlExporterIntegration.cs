using ToSic.Eav.Context;

namespace ToSic.Eav.ImportExport.Sys.XmlExport;

public interface IXmlExporterIntegration: IServiceWithSetup<IContextOfApp>
{
    public List<int> ReferencedFileIds { get; }
    public List<int> ReferencedFolderIds { get; }

    public string ResolveFolderId(int folderId);
    public TenantFileItem ResolveFile(int fileId);
    void AddFilesToExportQueue();
    void AddFileAndFolderToQueue(int fileNum);

}
