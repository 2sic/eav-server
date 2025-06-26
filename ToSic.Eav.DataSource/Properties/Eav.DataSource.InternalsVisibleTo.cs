using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ToSic.Eav.DataSources")]

// This is needed for everything to work
[assembly: InternalsVisibleTo("ToSic.Eav.Apps")]
[assembly: InternalsVisibleTo("ToSic.Eav.Work")]

// Tests
[assembly: InternalsVisibleTo("ToSic.Eav.DataSource.TestHelpers")]
[assembly: InternalsVisibleTo("ToSic.Eav.DataSource.Tests")]
[assembly: InternalsVisibleTo("ToSic.Eav.DataSource.DbTests")]
