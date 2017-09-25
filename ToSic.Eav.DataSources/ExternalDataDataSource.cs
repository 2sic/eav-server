using System;

namespace ToSic.Eav.DataSources
{
    public class ExternalDataDataSource: BaseDataSource
    {
        public override string LogId => "DS-Ext";

        public ExternalDataDataSource(): base()
        {
            // set the creation date to the moment the object is constructed
            // this is important, because the date should stay fixed throughout the lifetime of this object
            // but renew when it is updates
            CacheLastRefresh = DateTime.Now;
        }
 
        public override DateTime CacheLastRefresh { get; }
    }
}
