using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.App
{
    public class AppDataPackageDeferredList: IDeferredEntitiesList
    {
        public void AttachApp(AppDataPackage app) => _app = app;
        private AppDataPackage _app;

        public IDictionary<int, IEntity> List => _app.Entities;

        public IEnumerable<IEntity> LightList => _app.List;
    }
}
