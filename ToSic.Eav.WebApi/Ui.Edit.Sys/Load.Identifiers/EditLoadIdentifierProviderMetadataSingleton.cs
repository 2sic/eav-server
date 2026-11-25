using ToSic.Eav.Metadata;
using ToSic.Eav.Metadata.Targets;
using ToSic.Sys.HookUp;

namespace ToSic.Eav.Ui.Edit.Sys;

/// <summary>
/// Process headers which instruct the backend to get the metadata of a specific type.
/// </summary>
/// <remarks>
/// Some headers can simply request for metadata of type _SomeMetadata_ and the target.
/// This helper will figure out what that data is - or if new metadata should be created.
///
/// Note: this feature is still regarded as internal, it's only used in a few places and not really official; but it's been solid for years.
/// </remarks>
/// <param name="mdTargetTypes"></param>
internal class EditLoadIdentifierProviderMetadataSingleton(ITargetTypeService mdTargetTypes)
    : ServiceWithSetup<IAppWorkContext>("UoW.AddCtx", connect: [mdTargetTypes]),
        IEditLoadIdentifierProvider
{
    public int WorkSequenceOrder => 999; // fairly late

    public Task<Package<List<ItemIdentifier>>> Handle(WorkContext mainCtx, Package<List<ItemIdentifier>> package)
    {
        var appCtx = mainCtx.TryGet<IAppWorkContext>(EditLoadContextConstants.AppWorkCtx)
            ?? MyOptions;
        return Task.FromResult(package.RePackage(TryToAutoFindMetadataSingleton(appCtx, package.Data)));
    }


    /// <summary>
    /// new 2020-12-08 - correct entity-id with lookup of existing if marked as singleton
    /// </summary>
    // ReSharper disable once UnusedMethodReturnValue.Local
    private List<ItemIdentifier> TryToAutoFindMetadataSingleton(IAppWorkContext appCtx, List<ItemIdentifier> slots)
    {
        IMetadataSource appMdSource = appCtx.AppReader.Metadata;
        var l = Log.Fn<List<ItemIdentifier>>();

        var result = slots
            .Select(header =>
            {
                // Check if it applies - header must proclaim singleton, and must have a content type name, otherwise we can't do anything
                var mustUpdate = header.For?.Singleton == true && header.ContentTypeName.HasValue();
                if (!mustUpdate)
                    return header;

                l.A("Found an entity with the auto-lookup marker");
                
                // Check what the header declares what the target is (for whom the metadata should be)
                var mdFor = header.For;

                // Figure out the TargetTypeId (Entity, Content-Type, etc.)
                // #TargetTypeIdInsteadOfTarget
                var targetTypeId = mdFor!.TargetType != 0
                    ? mdFor.TargetType
                    : mdTargetTypes.GetId(mdFor.Target!);

                // Find all matching metadata for this; varies by whether we have a guid, number or string
                var mds = mdFor.Guid != null
                    ? appMdSource.GetMetadata(targetTypeId, mdFor.Guid.Value, header.ContentTypeName)
                    : mdFor.Number != null
                        ? appMdSource.GetMetadata(targetTypeId, mdFor.Number.Value, header.ContentTypeName)
                        : appMdSource.GetMetadata(targetTypeId, mdFor.String, header.ContentTypeName);


                var mdList = mds.ToArray();
                if (mdList.Length > 1)
                {
                    l.A($"Warning - looking for best metadata but found too many {mdList.Length}, will use first");
                    // must now sort by ID otherwise the order may be different after a few save operations
                    mdList = [.. mdList.OrderBy(e => e.EntityId)];
                }

                return header with { EntityId = !mdList.Any() ? 0 : mdList.First().EntityId };
            })
            .ToList();

        return l.Return(result);
    }

}
