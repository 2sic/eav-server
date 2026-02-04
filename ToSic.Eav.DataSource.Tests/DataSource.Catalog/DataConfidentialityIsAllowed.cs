using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.DataSource.VisualQuery.Sys;
using ToSic.Sys.Users;

namespace ToSic.Eav.DataSource.Catalog;

/// <summary>
/// Test if the check of data-source confidentiality levels works.
/// </summary>
public class DataConfidentialityIsAllowed
{

    [Theory]
    [InlineData(DataConfidentiality.Unknown)]
    [InlineData(DataConfidentiality.Never, false)]
    [InlineData(DataConfidentiality.System)]
    [InlineData(DataConfidentiality.Secret)]
    [InlineData(DataConfidentiality.Confidential)]
    [InlineData(DataConfidentiality.Internal)]
    [InlineData(DataConfidentiality.Public)]
    public void SystemAdmin(DataConfidentiality confidentiality, bool expected = true) =>
        Equal(expected, confidentiality.IsAllowed(new UserMock
        {
            IsSystemAdmin = true
        }));

    [Theory]
    [InlineData(DataConfidentiality.Unknown, false)]
    [InlineData(DataConfidentiality.Never, false)]
    [InlineData(DataConfidentiality.System, false)]
    [InlineData(DataConfidentiality.Secret, false)]
    [InlineData(DataConfidentiality.Confidential, true)]
    [InlineData(DataConfidentiality.Internal, true)]
    [InlineData(DataConfidentiality.Public, true)]
    public void SiteAdmin(DataConfidentiality confidentiality, bool expected) =>
        Equal(expected, confidentiality.IsAllowed(new UserMock
        {
            IsSiteAdmin = true
        }));

    [Theory]
    [InlineData(DataConfidentiality.Unknown, false)]
    [InlineData(DataConfidentiality.Never, false)]
    [InlineData(DataConfidentiality.System, false)]
    [InlineData(DataConfidentiality.Secret, false)]
    [InlineData(DataConfidentiality.Confidential, false)]
    [InlineData(DataConfidentiality.Internal, true)]
    [InlineData(DataConfidentiality.Public, true)]
    public void ContentAdmin(DataConfidentiality confidentiality, bool expected) =>
        Equal(expected, confidentiality.IsAllowed(new UserMock
        {
            IsContentAdmin = true
        }));

    [Theory]
    [InlineData(DataConfidentiality.Unknown, false)]
    [InlineData(DataConfidentiality.Never, false)]
    [InlineData(DataConfidentiality.System, false)]
    [InlineData(DataConfidentiality.Secret, false)]
    [InlineData(DataConfidentiality.Confidential, false)]
    [InlineData(DataConfidentiality.Internal, true)]
    [InlineData(DataConfidentiality.Public, true)]
    public void ContentEditor(DataConfidentiality confidentiality, bool expected) =>
        Equal(expected, confidentiality.IsAllowed(new UserMock
        {
            IsContentAdmin = true
        }));

    [Theory]
    [InlineData(DataConfidentiality.Unknown, false)]
    [InlineData(DataConfidentiality.Never, false)]
    [InlineData(DataConfidentiality.System, false)]
    [InlineData(DataConfidentiality.Secret, false)]
    [InlineData(DataConfidentiality.Confidential, false)]
    [InlineData(DataConfidentiality.Internal, false)]
    [InlineData(DataConfidentiality.Public, true)]
    public void Anonymous(DataConfidentiality confidentiality, bool expected) =>
        Equal(expected, confidentiality.IsAllowed(new UserMock()));

}
