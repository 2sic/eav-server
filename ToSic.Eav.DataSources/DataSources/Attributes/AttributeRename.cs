using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using static ToSic.Eav.DataSource.DataSourceConstants;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources;

/// <inheritdoc />
/// <summary>
/// DataSource to rename attributes. Will help to change internal field names to something which is more appropriate for your JS or whatever.
/// </summary>
[PublicApi_Stable_ForUseInYourCode]
[VisualQuery(
    NiceName = "Rename Attribute/Property",
    UiHint = "Rename some attributes / properties",
    Icon = Icons.EditAttributes,
    Type = DataSourceType.Modify,
    NameId = "ToSic.Eav.DataSources.AttributeRename, ToSic.Eav.DataSources",
    DynamicOut = false,
    In = new[] { InStreamDefaultRequired },
    ConfigurationType = "c5918cb8-d35a-48c7-9380-a437edde66d2",
    HelpLink = "https://go.2sxc.org/DsAttributeRename")]

public class AttributeRename : Eav.DataSource.DataSourceBase
{
    #region Configuration-properties

    /// <summary>
    /// A string containing one or more attribute maps.
    /// The syntax is "NewName=OldName" - one mapping per line
    /// </summary>
    [Configuration]
    public string AttributeMap
    {
        get => Configuration.GetThis();
        set => Configuration.SetThisObsolete(value);
    }

    /// <summary>
    /// True/false if attributes not renamed should be preserved.
    /// </summary>
    [Configuration(Fallback = true)]
    public bool KeepOtherAttributes
    {
        get => Configuration.GetThis(true);
        set => Configuration.SetThisObsolete(value);
    }

    /// <summary>
    /// A string containing one or more attribute maps.
    /// The syntax is "NewName=OldName" - one mapping per line
    /// </summary>
    [Configuration]
    public string TypeName
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
    public AttributeRename(DataBuilder dataBuilder, MyServices services) : base(services, $"{LogPrefix}.AtrRen")
    {
        ConnectServices(
            _dataBuilder = dataBuilder
        );
        ProvideOut(GetList);
    }

    private readonly DataBuilder _dataBuilder;


    /// <summary>
    /// Get the list of all items with reduced attributes-list
    /// </summary>
    /// <returns></returns>
    private IImmutableList<IEntity> GetList()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        Configuration.Parse();

        var mapRaw = AttributeMap;
        var attrMapArray = mapRaw
            .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .ToList();
        var attributeNames = attrMapArray
            .Select(s =>
            {
                var splitEquals = s.Split('=');
                if (splitEquals.Length != 2) return null;
                return splitEquals.Any(string.IsNullOrWhiteSpace)
                    ? null
                    : new { New = splitEquals[0].Trim(), Old = splitEquals[1].Trim() };
            })
            .Where(x => x != null)
            .ToList();

        var preserveOthers = KeepOtherAttributes;

        IDictionary<string, IAttribute> CreateDic(IEntity original)
        {
            return original.Attributes
                .Select(a =>
                {
                    // find in the map
                    var fieldMap = attributeNames.FirstOrDefault(an =>
                        string.Equals(an.Old, a.Key, StringComparison.InvariantCultureIgnoreCase));
                    if (fieldMap != null)
                    {
                        // check if the name actually changed, if not, return original (faster)
                        if (string.Equals(fieldMap.New, fieldMap.Old, StringComparison.InvariantCultureIgnoreCase))
                            return a;
                        var renameAttribute = CloneAttributeAndRename(a.Value, fieldMap.New);
                        return new KeyValuePair<string, IAttribute>(fieldMap.New, renameAttribute);
                    }

                    return preserveOthers
                        ? a
                        : new KeyValuePair<string, IAttribute>(null, null);
                })
                .Where(set => set.Key != null)
                .ToDictionary(a => a.Key, v => v.Value);

        }


        // build type if we have another one
        var typeName = TypeName;
        IContentType newType = null;
        if (!string.IsNullOrEmpty(typeName))
            newType = _dataBuilder.ContentType.Transient(AppId, typeName, typeName);

        var source = TryGetIn();
        if (source is null) return l.ReturnAsError(Error.TryGetInFailed());

        var result = source
            .Select(entity =>
            {
                var values = CreateDic(entity);
                return _dataBuilder.Entity.CreateFrom(entity, attributes: _dataBuilder.Attribute.Create(values), type: newType);
            })
            .ToImmutableList();

        return l.Return(result, $"attrib filter names:[{string.Join(",", attributeNames)}] found:{result.Count}");
    }



    private IAttribute CloneAttributeAndRename(IAttribute original, string newName)
    {
        var newAttrib = _dataBuilder.Attribute.Create(newName, original.Type, original.Values.ToList());
        // #immutable
        //newAttrib.Values = original.Values;
        return newAttrib;
    }

}