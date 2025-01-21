using static ToSic.Eav.DataSource.DataSourceConstants;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources;

/// <inheritdoc />
/// <summary>
/// Filter entities to show only these belonging to a specific user. 
/// </summary>
[PublicApi]
[VisualQuery(
    NiceName = "Owner Filter",
    UiHint = "Keep only item owned / created by a specified user",
    Icon = DataSourceIcons.PersonCircled,
    Type = DataSourceType.Security,
    NameId = "ToSic.Eav.DataSources.OwnerFilter, ToSic.Eav.DataSources",
    DynamicOut = false,
    In = [InStreamDefaultRequired],
    ConfigurationType = "|Config ToSic.Eav.DataSources.OwnerFilter",
    HelpLink = "https://go.2sxc.org/DsOwnerFilter")]

// ReSharper disable once UnusedMember.Global
public class OwnerFilter : DataSourceBase
{
    #region Configuration-properties

    private const string IdentityCodeField = "IdentityCode";

    /// <summary>
    /// The identity of the user to filter by. Uses the Identity-token convention like dnn:1 is the user #1 in the DNN DB
    /// </summary>
    [Configuration(Field = IdentityCodeField)]
    public string Identity
    {
        get => Configuration.GetThis();
        set => Configuration.SetThisObsolete(value);
    }
    #endregion

    /// <inheritdoc />
    /// <summary>
    /// Constructs a new PublishingFilter
    /// </summary>
    [PrivateApi]
    public OwnerFilter(MyServices services): base(services, $"{DataSourceConstantsInternal.LogPrefix}.OwnrFl")
    {
        ProvideOut(GetList);
    }

    private IImmutableList<IEntity> GetList()
    {
        // Get once to not re-access the property every time
        var identity = Identity;

        var l = Log.Fn<IImmutableList<IEntity>>($"get for identity:{identity}");

        if (string.IsNullOrWhiteSpace(identity))
            return l.ReturnAsError([], "no identity");

        var source = TryGetIn();
        return source is null
            ? l.ReturnAsError(Error.TryGetInFailed())
            : l.ReturnAsOk(source
                .Where(e => e.Owner == identity)
                .ToImmutableList()
            );
    }

}