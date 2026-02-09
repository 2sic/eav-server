using static ToSic.Eav.DataSource.DataSourceConstants;
using static ToSic.Eav.DataSource.Linking.DsLinkingTestData;

namespace ToSic.Eav.DataSource.Linking;

public class DsLinkRename
{

    [Fact]
    public void RenameOutLinkIsNewInstance()
    {
        var link = GetWithMockDataSource();
        Equal(StreamDefaultName, link.OutNameTac());
        var renamedOut = link.WithRenameTac(outName: Rename);
        NotNull(renamedOut);
        Equal(Rename, renamedOut.OutNameTac());
        NotEqual(link, renamedOut);
    }

    [Fact]
    public void RenameInLinkIsNewInstance()
    {
        var link = GetWithMockDataSource();
        Equal(StreamDefaultName, link.InNameTac());
        var renamedIn = link.WithRenameTac(inName: Rename);
        NotNull(renamedIn);
        Equal(Rename, renamedIn.InNameTac());
        NotEqual(link, renamedIn);
    }

    [Fact]
    public void RenameSameLinkIsSameInstance()
    {
        var link = GetWithMockDataSource();
        Equal(StreamDefaultName, link.OutNameTac());
        Equal(StreamDefaultName, link.InNameTac());
        var renamedSame = link.WithRename();
        NotNull(renamedSame);
        Equal(link, renamedSame);
        Equal(StreamDefaultName, renamedSame.OutNameTac());
        Equal(StreamDefaultName, renamedSame.InNameTac());
    }
}
