using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToSic.Eav.Repository.Interfaces
{
    public interface IRepoApp
    {
        int AppId { get; set; }
        int ZoneId { get; set; }
        string Name { get; set; }

    }
}
