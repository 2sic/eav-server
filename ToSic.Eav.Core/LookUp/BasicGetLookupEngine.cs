using ToSic.Eav.Logging;

namespace ToSic.Eav.LookUp
{
    /// <summary>
    /// Fall back implementation - just return an empty lookup engine
    /// This should usually be 
    /// </summary>
    public class BasicGetLookupEngine: HasLog<ILookUpEngineResolver>, ILookUpEngineResolver
    {
        public BasicGetLookupEngine() : base($"{LogNames.NotImplemented}.LookUp")
        {
        }

        public ILookUpEngine GetLookUpEngine(int instanceId) => new LookUpEngine(Log);
    }
}
