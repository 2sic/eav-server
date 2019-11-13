using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Persistence.Efc
{
    public partial class Efc11Loader
    {
        #region Testing / Analytics helpers

        //internal void ResetCacheForTesting()
        //    => _contentTypes = new Dictionary<int, IList<IContentType>>();
        #endregion

        #region Load Content-Types into IContent-Type Dictionary

        //private Dictionary<int, IList<IContentType>> _contentTypes = new Dictionary<int, IList<IContentType>>();

        /// <inheritdoc />
        /// <summary>
        /// Get all ContentTypes for specified AppId. 
        /// If uses temporary caching, so if called multiple times it loads from a private field.
        /// </summary>
        public IList<IContentType> ContentTypes(int appId, IHasMetadataSource source)
        {
            return LoadContentTypesIntoLocalCache(appId, source);
            
        }


        /// <summary>
        /// Load DB content-types into loader-cache
        /// </summary>
        private ImmutableList<IContentType> LoadContentTypesIntoLocalCache(int appId, 
            IHasMetadataSource source)
        {
            // Load from DB
            var sqlTime = Stopwatch.StartNew();
            var query = _dbContext.ToSicEavAttributeSets
                .Where(set => set.AppId == appId && set.ChangeLogDeleted == null);

            var contentTypes = query
                    .Include(set => set.ToSicEavAttributesInSets)
                        .ThenInclude(attrs => attrs.Attribute)
                    .Include(set => set.App)
                    .Include(set => set.UsesConfigurationOfAttributeSetNavigation)
                        .ThenInclude(master => master.App)
                    .ToList()
                    .Select(set => new
                    {
                        set.AttributeSetId,
                        set.Name,
                        set.StaticName,
                        set.Scope,
                        set.Description,
                        Attributes = set.ToSicEavAttributesInSets
                            .Where(a => a.Attribute.ChangeLogDeleted == null) // only not-deleted attributes!
                            .Select(a => new ContentTypeAttribute(appId, a.Attribute.StaticName, a.Attribute.Type, a.IsTitle, a.AttributeId, a.SortOrder, source)),
                        IsGhost = set.UsesConfigurationOfAttributeSet,
                        SharedDefinitionId = set.UsesConfigurationOfAttributeSet,
                        AppId = set.UsesConfigurationOfAttributeSetNavigation?.AppId ?? set.AppId,
                        ZoneId = set.UsesConfigurationOfAttributeSetNavigation?.App?.ZoneId ?? set.App.ZoneId,
                        ConfigIsOmnipresent =
                        set.UsesConfigurationOfAttributeSetNavigation?.AlwaysShareConfiguration ?? set.AlwaysShareConfiguration,
                    })
                .ToList();
            sqlTime.Stop();

            var shareids = contentTypes.Select(c => c.SharedDefinitionId).ToList();
            sqlTime.Start();
            var sharedAttribs = _dbContext.ToSicEavAttributeSets
                .Include(s => s.ToSicEavAttributesInSets)
                .ThenInclude(a => a.Attribute)
                .Where(s => shareids.Contains(s.AttributeSetId))
                .ToDictionary(s => s.AttributeSetId, s => s.ToSicEavAttributesInSets.Select(a
                    => new ContentTypeAttribute(appId, a.Attribute.StaticName, a.Attribute.Type, a.IsTitle,
                        a.AttributeId, a.SortOrder, parentApp: s.AppId)));
            sqlTime.Stop();

            // Convert to ContentType-Model
            var newTypes = contentTypes.Select(set => (IContentType) new ContentType(appId, set.Name, set.StaticName, set.AttributeSetId,
                    set.Scope, set.Description, set.IsGhost, set.ZoneId, set.AppId, set.ConfigIsOmnipresent, source)
                {
                    Attributes = (set.SharedDefinitionId.HasValue
                            ? sharedAttribs[set.SharedDefinitionId.Value]
                            : set.Attributes)
                        // ReSharper disable once RedundantEnumerableCastCall
                        .Cast<IContentTypeAttribute>()
                        .ToList()
                }
            );

            //if (justAddNewOnes && prevList != null)
            //    newTypes = newTypes.Concat(prevList);

            _sqlTotalTime = _sqlTotalTime.Add(sqlTime.Elapsed);

            //_contentTypes[appId] =
            return newTypes.ToImmutableList();

        }

        #endregion



    }
}
