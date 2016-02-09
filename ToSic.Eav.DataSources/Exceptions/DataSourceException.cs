using System;

namespace ToSic.Eav.DataSources.Exceptions
{
    public class DataSourceException: System.Exception
    {
        private string p;
        private Exception ex;

        public DataSourceException(string p)
        {
            // TODO: Complete member initialization
            this.p = p;
        }

        public DataSourceException(string p, Exception ex)
        {
            // TODO: Complete member initialization
            this.p = p;
            this.ex = ex;
        }
    }
}
