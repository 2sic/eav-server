using ToSic.Eav.Data.Sys.ValueConverter;
using ToSic.Eav.Ui.Edit.Sys.Load.Links;
using ToSic.Sys.HookUp;

namespace ToSic.Eav.Ui.Edit.Sys.Load.Prefetch;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class EditLoadActivityPrefetchLinks(Generator<ILinkResolver> hyperlinkBackend)
    : ServiceBase("Api.Prefetch", connect: [hyperlinkBackend]),
        IEditLoadPrefetchProvider
{
    public const string NameId = "Links";

    public Task<Package<object>> Handle(WorkContext mainCtx, Package<EditLoadDto> package)
    {
        var result = PrefetchLinks(mainCtx.Get<int>(EditLoadContextConstants.AppId), package.Data);
        return Task.FromResult(package.RePackage(result as object));
    }

    public Dictionary<string, LinkInfoDto> PrefetchLinks(int appId, EditLoadDto editData)
    {
        try
        {
            // Step 1: try to find hyperlink fields
            var bundlesHavingLinks = BundleWithLinkField.ExtractLinkFields(editData);

            var links = bundlesHavingLinks
                .SelectMany(set => (set.HyperlinkFields ?? [])
                    .SelectMany(h => (h.Value ?? [])
                        .Select(linkAttrib => new
                    {
                        Link = linkAttrib.Value,
                        Converted = TryToConvertOrErrorText(new(ContentType: set.ContentTypeName, Field: h.Key, Hyperlink: linkAttrib.Value, Entity: set.Guid)),
                    })))
                //.Where(set => set != null)
                // Step 2: Check which ones have a link reference
                .Where(set => ValueConverterBase.CouldBeReference(set.Link))
                // Make distinct by Link
                .GroupBy(l => l.Link)
                .Select(g => g.First())
                .ToList();


            // Step 3: Look them up
            // Step 4: return dictionary with these
            return links.ToDictionary(
                s => s.Link,
                s => s.Converted);
        }
        catch (Exception ex)
        {
            return new()
            {
                {"Error", new() { Value = "An error occurred pre-fetching the links " + ex.Message } }
            };
        }
    }

    private LinkInfoDto TryToConvertOrErrorText(ILinkResolver.Parameters parameters)
    {
        try
        {
            return LinkResolver.Handle(new(), parameters.ToPackage()).GetAwaiter().GetResult().Data;
        }
        catch
        {
            return new() { Value = "error" };
        }
    }

    private ILinkResolver LinkResolver => field ??= hyperlinkBackend.New();
}
