using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Internal;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using static System.StringComparer;
using static ToSic.Eav.DataSource.Internal.DataSourceConstants;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources;

/// <inheritdoc />
/// <summary>
/// DataSource to only pass through configured AttributeNames - other attributes/properties are removed from the entities.
/// </summary>
[PublicApi]
[VisualQuery(
    NiceName = "Remove Attribute/Property",
    UiHint = "Remove attributes/properties to limit what is available",
    Icon = DataSourceIcons.Delete,
    Type = DataSourceType.Modify, 
    NameId = "ToSic.Eav.DataSources.AttributeFilter, ToSic.Eav.DataSources",
    DynamicOut = false,
    In = new [] { InStreamDefaultRequired },
    ConfigurationType = "|Config ToSic.Eav.DataSources.AttributeFilter",
    HelpLink = "https://go.2sxc.org/DsAttributeFilter")]

public class AttributeFilter : Eav.DataSource.DataSourceBase
{
    #region Constants

    [PrivateApi] internal const string ModeKeep = "+";
    [PrivateApi] internal const string ModeRemove = "-";

    #endregion
        
    #region Configuration-properties
        
    /// <summary>
    /// A string containing one or more attribute names. like "FirstName" or "FirstName,LastName,Birthday"
    /// </summary>
    [Configuration]
    public string AttributeNames
    {
        get => Configuration.GetThis();
        set => Configuration.SetThisObsolete(value);
    }
        
    /// <summary>
    /// A string containing one or more attribute names. like "FirstName" or "FirstName,LastName,Birthday"
    /// </summary>
    [Configuration(Fallback = ModeKeep)]
    public string Mode
    {
        get => Configuration.GetThis();
        set => Configuration.SetThisObsolete(value);
    }
      

    #endregion

    /// <inheritdoc />
    /// <summary>
    /// Constructs a new AttributeFilter DataSource
    /// </summary>
    [PrivateApi]
    public AttributeFilter(EntityBuilder entityBuilder, MyServices services): base(services, $"{LogPrefix}.AtribF")
    {
        _entityBuilder = entityBuilder;
        ProvideOut(GetList);
    }

    private readonly EntityBuilder _entityBuilder;


    /// <summary>
    /// Get the list of all items with reduced attributes-list
    /// </summary>
    /// <returns></returns>
    private IImmutableList<IEntity> GetList()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        Configuration.Parse();

        var source = TryGetIn();
        if (source is null) return l.ReturnAsError(Error.TryGetInFailed());

        var raw = AttributeNames;
        // note: since 2sxc 11.13 we have lines for attributes
        // older data still uses commas since it was single-line
        var attributeNames = raw.Split(raw.Contains("\n") ? '\n' : ',');
        attributeNames = attributeNames
            .Select(a => a.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();

        l.A($"attrib filter names:[{string.Join(",", attributeNames)}]");

        // Determine if we should remove or keep the things in the list
        var modeIsKeepAttributes = Mode != ModeRemove;

        // If no attributes were given or just one with *, then don't filter at all
        var noFieldNames = attributeNames.Length == 0
                           || attributeNames.Length == 1 && string.IsNullOrWhiteSpace(attributeNames[0]);

        // Case #1 if we don't change anything, short-circuit and return original
        if (noFieldNames && !modeIsKeepAttributes)
            return l.Return(source, $"keep original {source.Count}");

        var result = source
            .Select(e =>
            {
                // Case 2: Check if we should take none at all
                if (noFieldNames && modeIsKeepAttributes)
                    return _entityBuilder.CreateFrom(e, attributes: _entityBuilder.Attribute.Empty());

                // Case 3 - not all fields, keep/drop the ones we don't want
                var attributes = e.Attributes
                    .Where(aPair => attributeNames.Contains(aPair.Key) == modeIsKeepAttributes)
                    .ToImmutableDictionary(pair => pair.Key, pair => pair.Value, InvariantCultureIgnoreCase);
                return _entityBuilder.CreateFrom(e, attributes: attributes);
            })
            .ToImmutableList();

        return l.Return(result, $"modified {result.Count}");
    }

}