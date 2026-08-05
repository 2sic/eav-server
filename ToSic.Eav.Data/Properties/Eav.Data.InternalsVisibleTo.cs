using System.Runtime.CompilerServices;

// The build API must be able to access some internal stuff
[assembly: InternalsVisibleTo("ToSic.Eav.Data.Build")]

// Other parts which need internals
[assembly: InternalsVisibleTo("ToSic.Eav.Apps")]
[assembly: InternalsVisibleTo("ToSic.Eav.Persistence")]

// Data Tests which need internals
[assembly: InternalsVisibleTo("ToSic.Eav.Data.Tests")]
[assembly: InternalsVisibleTo("ToSic.Eav.Data.TestHelpers")]
[assembly: InternalsVisibleTo("ToSic.Eav.Data.Build.Tests")]
[assembly: InternalsVisibleTo("ToSic.Eav.Models.Tests")]
[assembly: InternalsVisibleTo("ToSic.Eav.Data.TestsPostBuild")]

// DataSource Tests which need internals
[assembly: InternalsVisibleTo("ToSic.Eav.DataSource.Tests")]
[assembly: InternalsVisibleTo("ToSic.Eav.DataSources.Tests")]
