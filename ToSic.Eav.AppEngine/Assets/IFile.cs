using System;

namespace ToSic.Eav.Apps.Assets
{
    public interface IFile
    {
        DateTime Created { get; set; }
        string Extension { get; set; }
        string Folder { get; set; }
        int FolderId { get; set; }
        string FullName { get; set; }
        int Id { get; set; }
        DateTime Modified { get; set; }
        string Path { get; set; }
        int Size { get; set; }
    }
}