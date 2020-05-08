using System.Collections.Generic;
using ToSic.Eav.Apps.Parts;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Run
{
    public interface IAppFileSystemLoader
    {
        string Path { get; set; }

        List<InputTypeInfo> FindInputTypes();
    }
}
