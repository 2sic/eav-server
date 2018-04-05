using System;

namespace ToSic.Eav.Apps.Assets
{
    public interface IFolder
    {
        DateTime Created { get; set; }
        string FolderPath { get; set; }
        bool HasChildren { get; set; }
        int Id { get; set; }
        DateTime Modified { get; set; }
        string Name { get; set; }
    }
}