using ToSic.Eav.Data.PiggyBack;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    public partial class Entity: IHasPiggyBack
    {
        [PrivateApi("WIP Piggyback")]
        public PiggyBack.PiggyBack PiggyBack => _piggyBack ?? (_piggyBack = new PiggyBack.PiggyBack());
        private PiggyBack.PiggyBack _piggyBack;
    }
}
