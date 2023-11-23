using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Sys.Types;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources.Sys
{
    public class ContentTypeUtil
    {
        internal static Dictionary<string, object> BuildDictionary(IContentType t) => new()
        {
            { ContentTypeType.Name.ToString(), t.Name },
            { ContentTypeType.StaticName.ToString(), t.NameId },
            { nameof(t.NameId), t.NameId },
            { ContentTypeType.IsDynamic.ToString(), t.IsDynamic },

            { ContentTypeType.Scope.ToString(), t.Scope },
            { ContentTypeType.AttributesCount.ToString(), t.Attributes.Count() },

            { ContentTypeType.RepositoryType.ToString(), t.RepositoryType.ToString() },
            { ContentTypeType.RepositoryAddress.ToString(), t.RepositoryAddress },
        };
    }
}