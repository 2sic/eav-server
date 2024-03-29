﻿using ToSic.Eav.Data.Shared;

namespace ToSic.Eav.Apps.Internal.Work;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class WorkAttributes() : WorkUnitBase<IAppWorkCtx>("Wrk.Attrib")
{
    public List<PairTypeWithAttribute> GetFields(string staticName)
    {
        var l = Log.Fn<List<PairTypeWithAttribute>>($"a#{AppWorkCtx.Show()}, type:{staticName}");

        if (AppWorkCtx.AppState.GetContentType(staticName) is not { } type)
            return l.Return([],
                $"error, type:{staticName} is null, it is missing or it is not a ContentType - something broke");

        var fields = type.Attributes.OrderBy(a => a.SortOrder);

        return l.Return(fields.Select(a => new PairTypeWithAttribute { Type = type, Attribute = a }).ToList());
    }

    public List<PairTypeWithAttribute> GetSharedFields(int attributeId)
    {
        var l = Log.Fn<List<PairTypeWithAttribute>>($"get shared fields {AppWorkCtx.Show()}");
        var appState = AppWorkCtx.AppState;
        var localTypes = appState.ContentTypes
            .Where(ct => !ct.HasAncestor())
            .ToList();

        // If we know what AttributeId it's for, then filter
        var attributeType = ValueTypes.Undefined;
        if (attributeId != default)
        {
            var attribute = appState.ContentTypes
                .SelectMany(ct => ct.Attributes.Where(a => a.AttributeId == attributeId))
                .FirstOrDefault();
                
            if (attribute == null)
                return l.Return([], $"attribute {attributeId} not found");

            attributeType = attribute.Type;
        }

        var fields = localTypes
            .SelectMany(ct => ct.Attributes
                .Where(a =>
                    a.AttributeId != 0 // don't take AttributeId 0 which would be file-system based (not in DB)
                    && a.Guid != null // Guid required for sharing
                    && a.SysSettings?.Share == true // Share must be defined
                    && (attributeType == ValueTypes.Undefined || a.Type == attributeType)
                )
                .Select(a => new PairTypeWithAttribute { Type = ct, Attribute = a }))
            .OrderBy(set => set.Type.Name)
            .ThenBy(set => set.Attribute.Name)
            .ToList();

        return l.Return(fields);
    }
}