using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.Api.Api01;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Security.Permissions;
using ToSic.Lib.Logging;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Api.Api01
{
    partial class SimpleDataController
    {

        private EntitySavePublishing? FigureOutPublishing(IContentType contentType, IDictionary<string, object> values, bool? existingIsPublished)
        {
            var l = Log.Fn<EntitySavePublishing?>($"..., ..., attributes: {values?.Count}");
            // (bool ShouldPublish, bool DraftShouldBranch, EntitySavePublishing Publishing)? publishAndBranch = null;
            if (values.SafeNone())
                return l.Return(null, "no attributes to process");

            // On update, by default preserve IsPublished state
            var isPublished = existingIsPublished ?? true;

            // Ensure WritePublished or WriteDraft user permissions. 
            var allowed = GetWriteAndPublishAllowed(contentType);
            if (!allowed.WriteAllowed)
                throw l.Ex(new Exception("User is not allowed to do anything. Both published and draft are not allowed."));

            // IsPublished becomes false when write published is not allowed.
            if (isPublished && !allowed.PublishAllowed) isPublished = false;

            // Find publishing instructions
            // Handle special "PublishState" attribute
            var publishKvp = values.FirstOrDefault(pair => pair.Key.EqualsInsensitive(SaveApiAttributes.SavePublishingState));

            // did it exist? must check _key_, because kvps don't have a null-default
            if (publishKvp.Key == default)
                return l.Return(null, $"done, param {SaveApiAttributes.SavePublishingState} not provided");

            var publishAndBranch = GetPublishSpecs(publishedState: publishKvp.Value, existingIsPublished: isPublished, allowed.PublishAllowed, Log);

            return l.Return(publishAndBranch, "done");
        }


        #region Permission Checks

        private (bool PublishAllowed, bool WriteAllowed) GetWriteAndPublishAllowed(IContentType targetType)
        {
            var l = Log.Fn<(bool PublishAllowed, bool WriteAllowed)>();
            // skip write publish/draft permission checks for c# API
            if (!_checkWritePermissions) return l.ReturnAndLog((true, true), "skip write perm check - all ok");

            // this write publish/draft permission checks should happen only for REST API

            // 1. Find if user may write PUBLISHED:
            var appStateReader = _ctxWithDb.AppState;

            // 1.1. app permissions 
            if (_appPermissionCheckGenerator.New().ForAppInInstance(_ctx, appStateReader)
                .UserMay(GrantSets.WritePublished)) return l.ReturnAndLog((true, true), "App check - all ok");

            // 1.2. type permissions
            if (_appPermissionCheckGenerator.New().ForType(_ctx, appStateReader, targetType)
                .UserMay(GrantSets.WritePublished)) return l.ReturnAndLog((true, true), "Type check, all ok");


            // 2. Find if user may write DRAFT:

            // 2.1. app permissions 
            if (_appPermissionCheckGenerator.New().ForAppInInstance(_ctx, appStateReader)
                .UserMay(GrantSets.WriteDraft)) return l.ReturnAndLog((false, true), "App check draft - f/t");

            // 2.2. type permissions
            if (_appPermissionCheckGenerator.New().ForType(_ctx, appStateReader, targetType)
                .UserMay(GrantSets.WriteDraft)) return l.ReturnAndLog((false, true), "Type check draft - f/t");


            // 3. User is not allowed to update published or draft entity.
            return l.ReturnAndLog((false, false), "default: all not allowed");
        }


        #endregion
    }
}
