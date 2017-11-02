using ToSic.Eav.Enums;
using ToSic.Eav.ImportExport.Persistence.File;

namespace ToSic.Eav.ImportExport.Tests.Persistence.File
{
    public class DemoRuntime: Runtime
    {
        public DemoRuntime() : this("Tst.Runtim"){}

        public DemoRuntime(string logName) : base(logName)
        {
            Source = Repositories.TestFiles;
        }

        public override string Path => PathToUse;

        public static string PathToUse = "";
    }
}
