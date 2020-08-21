using ToSic.Eav.Logging;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Run
{
    public class ContainerNull: IContainer
    {
        public IContainer Init(int id, ILog parentLog)
        {
            // don't do anything
            return this;
        }

        public int Id => Constants.NullId;
        public bool IsPrimary => true;
        public IBlockIdentifier BlockIdentifier => null;
    }
}
