using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToSic.Eav.Data.Processing;

public interface IDataProcessorProcess
{
    Task<DataProcessorResult<IEntity?>> Process(IEntity entity);

}
