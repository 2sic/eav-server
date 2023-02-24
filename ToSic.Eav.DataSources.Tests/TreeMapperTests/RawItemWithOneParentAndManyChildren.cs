using System;
using System.Collections.Generic;
using ToSic.Eav.Data.New;

namespace ToSic.Eav.DataSourceTests.TreeMapperTests
{
    internal class NewItemWithOneParentAndManyChildren: INewEntity
    {
        public NewItemWithOneParentAndManyChildren(int id, Guid guid, int parentId, List<int> childrenIds)
        {
            Id = id;
            Guid = guid;
            Created = DateTime.Now;
            Modified = DateTime.Now;
            ParentId = parentId;
            ChildrenIds = childrenIds;
        }

        public int Id { get;  }
        public Guid Guid { get;  }
        public DateTime Created { get; }
        public DateTime Modified { get; }

        public string Title => $"Auto-Title {Id} / {Guid}";

        public int ParentId { get; }

        public List<int> ChildrenIds { get; }

        public Dictionary<string, object> GetProperties(CreateFromNewOptions options) => new Dictionary<string, object>
        {
            { nameof(Title), Title }
        };

    }
}
