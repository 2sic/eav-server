using System.Collections.Generic;
using ToSic.Eav.Data;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Apps;

public interface IAppContentTypeReader
{
    IEnumerable<IContentType> ContentTypes { get; }

    IContentType GetContentType(string name);
}