using System.Collections.Generic;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Repositories;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Run
{
    public interface IAppFileSystemLoader: IAppRepositoryLoader
    {
        string Path { get; set; }

        /// <summary>
        /// Load all the input types for this app from the folder
        /// </summary>
        /// <returns></returns>
        List<InputTypeInfo> InputTypes();

    }
}
