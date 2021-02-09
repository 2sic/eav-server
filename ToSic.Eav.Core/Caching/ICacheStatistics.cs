using System.Collections.Generic;

namespace ToSic.Eav.Caching
{
    /// <summary>
    /// WIP - trying to keep more information about cache changes
    /// </summary>
    public interface ICacheStatistics: ITimestamped
    {
        long FirstTimestamp { get; }
        
        Stack<long> History { get; }
        
        int ResetCount { get; }

        void Update(long newTimeStamp);
    }
}
