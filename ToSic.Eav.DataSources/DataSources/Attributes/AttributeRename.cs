﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
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
        GlobalName = "ToSic.Eav.DataSources.AttributeRename, ToSic.Eav.DataSources",
        DynamicOut = false,
        In = new[] { Constants.DefaultStreamNameRequired },
        ExpectsDataOfType = "c5918cb8-d35a-48c7-9380-a437edde66d2",
        HelpLink = "https://r.2sxc.org/DsAttributeRename")]

    public class AttributeRename : DataSource
	{
		#region Configuration-properties
		private const string AttributeMapKey = "AttributeMap";
		private const string KeepOtherAttributesKey = "KeepOtherAttributes";
        private const string TypeNameKey = "TypeName";

        /// <summary>
        /// A string containing one or more attribute maps.
        /// The syntax is "NewName=OldName" - one mapping per line
        /// </summary>
        public string AttributeMap
		{
		    get => Configuration[AttributeMapKey];
            set => Configuration[AttributeMapKey] = value;
        }

        /// <summary>
        /// True/false if attributes not renamed should be preserved.
        /// </summary>
        public bool KeepOtherAttributes
        {
            get => DataSourceConfiguration.TryConvertToBool(Configuration[KeepOtherAttributesKey]);
            set => Configuration[KeepOtherAttributesKey] = value.ToString();
        }

        /// <summary>
        /// A string containing one or more attribute maps.
        /// The syntax is "NewName=OldName" - one mapping per line
        /// </summary>
        public string TypeName
        {
            get => Configuration[TypeNameKey];
            set => Configuration[TypeNameKey] = value;
        }

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new AttributeFilter DataSource
        /// </summary>
        [PrivateApi]
		public AttributeRename(MultiBuilder multiBuilder, Dependencies dependencies) : base(dependencies, $"{DataSourceConstants.LogPrefix}.AtrRen")

        {
            _multiBuilder = multiBuilder;

            Provide(GetList);
			ConfigMask(AttributeMapKey, "[Settings:AttributeMap]");
			ConfigMask(KeepOtherAttributesKey, $"[Settings:KeepOtherAttributes||True]");
			ConfigMask(TypeNameKey, $"[Settings:TypeName]");
        }

        private readonly MultiBuilder _multiBuilder;


        /// <summary>
        /// Get the list of all items with reduced attributes-list
        /// </summary>
        /// <returns></returns>
        private IImmutableList<IEntity> GetList() => Log.Func(() =>
        {
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

            Dictionary<string, IAttribute> CreateDic(IEntity original)
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
                newType = new ContentTypeBuilder().Transient(AppId, typeName, typeName);

            if (!GetRequiredInList(out var originals))
                return (originals, "error");

            var result = originals
                .Select(entity => _multiBuilder.Entity.Clone(entity,
                    CreateDic(entity),
                    entity.Relationships.AllRelationships,
                    newType
                ))
                .Cast<IEntity>()
                .ToImmutableArray();

            Log.A($"attrib filter names:[{string.Join(",", attributeNames)}] found:{result.Length}");
            return (result, "ok");
        });



        private IAttribute CloneAttributeAndRename(IAttribute original, string newName)
        {
            var attributeType = DataTypes.GetAttributeTypeName(original);
            var newAttrib = _multiBuilder.Attribute.CreateTyped(newName, attributeType);
            newAttrib.Values = original.Values;
            return newAttrib;
        }

    }
}