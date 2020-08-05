using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToSic.Eav.Repository.Interfaces
{
    public interface IRepoValue
    {
        int ValueId { get; set; }
        int EntityId { get; set; }
        int AttributeId { get; set; }
        string Value { get; set; }
        int ChangeLogCreated { get; set; }
        int? ChangeLogDeleted { get; set; }
        int? ChangeLogModified { get; set; }

    }
}
