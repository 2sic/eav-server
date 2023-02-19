﻿using System;
using System.Collections.Generic;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.DataSourceTests.TreeMapperTests
{
    internal class RawItemWithOneParentAndManyChildren: IRawEntity
    {
        public RawItemWithOneParentAndManyChildren(int id, Guid guid, int parentId, List<int> childrenIds)
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

        public Dictionary<string, object> RawProperties => new Dictionary<string, object>()
        {
            { nameof(Title), Title }
        };
    }
}
