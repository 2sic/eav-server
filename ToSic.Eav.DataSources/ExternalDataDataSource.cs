using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToSic.Eav.DataSources
{
    public class ExternalDataDataSource: BaseDataSource
    {

        private DateTime objectCreated;

        public ExternalDataDataSource(): base()
        {
            // set the creation date to the moment the object is constructed
            // this is important, because the date should stay fixed throughout the lifetime of this object
            // but renew when it is updates
            objectCreated = DateTime.Now;
        }
 
        public override DateTime CacheLastRefresh
        {
            get { return objectCreated; }
        }
    }
}
