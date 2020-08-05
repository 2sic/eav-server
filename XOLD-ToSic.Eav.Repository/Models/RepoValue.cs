using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToSic.Eav.Repository.Interfaces;

namespace ToSic.Eav.Repository.Models
{
    public class RepoValue: IRepoValue
    {
        public int ValueId { get; set; }
        public int EntityId { get; set; }
        public int AttributeId { get; set; }
        public string Value { get; set; }
        public int ChangeLogCreated { get; set; }
        public int? ChangeLogDeleted { get; set; }
        public int? ChangeLogModified { get; set; }

    }
}
