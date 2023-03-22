using ToSic.Eav.Data.Build;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.DataSources
{
    public partial class TreeMapper : ServiceBase, ITreeMapper, ICanDebug
    {
        public const string DefaultParentFieldName = "Parent";
        public const string DefaultChildrenFieldName = "Children";

        #region Constructor / DI



        #endregion
        private readonly DataBuilder _builder;

        /// <summary>
        /// Constructor for DI
        /// </summary>
        /// <param name="builder"></param>
        public TreeMapper(DataBuilder builder): base("DS.TreeMp")
        {
            _builder = builder;
            Debug = false;
        }





        public bool Debug { get; set; }
    }


}