using System.Collections.Generic;

namespace ToSic.Eav.Caching
{
    public class CacheStatistics: ICacheStatistics
    {
        public long CacheTimestamp { get; private set; }
        
        public long FirstTimestamp { get; private set; }
        
        public Stack<long> History { get; } = new Stack<long>();

        public int ResetCount { get; private set; }

        public void Update(long newTimeStamp)
        {
            CacheTimestamp = newTimeStamp;
            History.Push(newTimeStamp);
            if (FirstTimestamp == 0) FirstTimestamp = newTimeStamp;
            else ResetCount++;
        }
    }
}
